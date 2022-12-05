using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChipBarUI : MonoBehaviour
{
    public static ChipBarUI instance;
    public Manager manager;

    public RectTransform bar;
    public GameObject chipButtonHolderPrefab;
    public CustomButton buttonPrefab;

    public float buttonSpacing = 15f;
    public float buttonWidthPadding = 10;

    public List<string> hideList;

    public Scrollbar horizontalScroll;
    public ScrollRect scrollRect;
    public Transform scrollRectViewport;
    public TMP_Dropdown FolderDropdown;
    public TMP_Text Laberl;


    public Sprite BuiltInSprite;
    public Sprite UserSprite;
    public Sprite newFolderIcon;

    public List<CustomButton> customButton = new List<CustomButton>();
    public Dictionary<int, (RectTransform Holder, int Value)> chipButtonHolders = new Dictionary<int, (RectTransform Holder, int Value)>();


    public static int CurrentFolderIndex = 0;
    TMP_Dropdown.OptionData newFolderOption = new TMP_Dropdown.OptionData("New Folder");

    void Awake()
    {
        instance = this;
        manager = FindObjectOfType<Manager>();
    }

    private void Start()
    {
        manager.customChipCreated += AddChipButton;
        manager.customChipUpdated += UpdateChipButton;
        newFolderOption = new TMP_Dropdown.OptionData()
        {
            text = "New Folder",
            image = newFolderIcon
        };
        FolderDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { newFolderOption });


        ReloadFolder();
        FolderDropdown.value = 1;
    }

    public void ReloadChipButton()
    {

        foreach (var button in customButton)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        customButton.Clear();

        foreach (var BuiltInChip in manager.builtinChips)
            AddChipButton(BuiltInChip);

        foreach (var Customchip in manager.SpawnableCustomChips)
            AddChipButton(Customchip);



        Canvas.ForceUpdateCanvases();
    }


    private void ReloadFolder()
    {
        foreach (var Holder in chipButtonHolders)
            DestroyImmediate(Holder.Value.Holder.gameObject);
        chipButtonHolders.Clear();

        FolderDropdown.options.Clear();

        foreach (var kv in FolderSystem.Enum)
            AddFolderView(kv.Key, kv.Key > 2 ? UserSprite : BuiltInSprite);

        ReloadChipButton();

    }

    public void NotifyRemovedFolder(string FolderName)
    {
        var DeletedWhileOnFolder = string.Equals(FolderDropdown.options[FolderDropdown.value].text, FolderName);

        ReloadFolder();
        if (DeletedWhileOnFolder)
            FolderDropdown.value = 0;

        FolderDropdown.onValueChanged?.Invoke(FolderDropdown.value);

    }

    public void NotifyFolderNameChanged()
    {
        ReloadFolder();
        FolderDropdown.onValueChanged?.Invoke(FolderDropdown.value);
        Laberl.text = FolderDropdown.options[FolderDropdown.value].text;
    }



    void LateUpdate() { UpdateBarPos(); }

    void UpdateBarPos()
    {
        float barPosY = (horizontalScroll.gameObject.activeSelf) ? 16 : 0;
        bar.localPosition = new Vector3(0, barPosY, 0);
    }

    void AddChipButton(Chip chip)
    {

        if (hideList.Contains(chip.chipName))
            return;

        ChipPackage package = chip.GetComponent<ChipPackage>();
        Transform holder;
        switch (package.chipType)
        {
            case ChipPackage.ChipType.Combapibility:
                holder = chipButtonHolders[(int)DefaultKays.Comp].Holder.transform;
                break;
            case ChipPackage.ChipType.Gate:
                holder = chipButtonHolders[(int)DefaultKays.Gate].Holder.transform;
                break;
            case ChipPackage.ChipType.Miscellaneous:
                holder = chipButtonHolders[(int)DefaultKays.Misc].Holder.transform;
                break;
            default:

                int index = (int)DefaultKays.Comp;
                if (chip is CustomChip customChip)
                {
                    if (FolderSystem.ContainsIndex(customChip.FolderIndex))
                        index = customChip.FolderIndex;
                }
                holder = chipButtonHolders[index].Holder.transform;
                break;
        }

        CustomButton button = Instantiate(buttonPrefab);
        button.gameObject.name = "Create (" + chip.chipName + ")";
        // Set button text
        var buttonTextUI = button.GetComponentInChildren<TMP_Text>();
        buttonTextUI.text = chip.chipName;

        // Set button size
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta =
            new Vector2(buttonTextUI.preferredWidth + buttonWidthPadding,
                        buttonRect.sizeDelta.y);

        // Set button position
        buttonRect.SetParent(holder, false);

        // Set button event
        button.AddListener(() => manager.ChipButtonHanderl(chip));

        customButton.Add(button);
    }

    void UpdateChipButton(Chip chip)
    {
        if (hideList.Contains(chip.chipName))
            return;

        CustomButton button =
            customButton.Find(g => g.name == "Create (" + chip.chipName + ")");
        if (button != null)
        {
            button.ClearEvents();
            button.AddListener(() => manager.ChipButtonHanderl(chip));
        }
    }

    public void SelectFolder()
    {
        if (FolderDropdown.value == FolderDropdown.options.Count - 1)
        {
            UIManager.instance.OpenMenu(MenuType.NewFolderMenu);
            FolderDropdown.value = chipButtonHolders[CurrentFolderIndex].Value; // TODO set Last Used Folder
            return;
        }

        CurrentFolderIndex = FolderSystem.ReverseIndex(FolderDropdown.options[FolderDropdown.value].text);

        foreach (var chipHolder in chipButtonHolders)
            chipHolder.Value.Holder.gameObject.SetActive(false);

        var HolderSelecter = chipButtonHolders[CurrentFolderIndex].Holder;
        HolderSelecter.gameObject.SetActive(true);

        scrollRect.content = HolderSelecter;
        UpdateBarPos();
    }

    public void AddFolderView(int FolderIndex, Sprite sprite)
    {
        var folderName = FolderSystem.GetFolderName(FolderIndex);
        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(folderName, sprite);


        FolderDropdown.options.Remove(newFolderOption);
        FolderDropdown.options.Add(newOption);
        FolderDropdown.options.Add(newFolderOption);
        NewChipButtonHolder(FolderIndex, folderName);
    }

    void NewChipButtonHolder(int FolderIdex, string FolderName)
    {
        RectTransform newHolder = Instantiate(chipButtonHolderPrefab).GetComponent<RectTransform>();
        newHolder.gameObject.name = FolderName + "Chips";
        newHolder.gameObject.SetActive(false);
        chipButtonHolders.Add(FolderIdex, (newHolder, FolderDropdown.options.Count-2));
        newHolder.SetParent(scrollRectViewport, false);
    }
}
