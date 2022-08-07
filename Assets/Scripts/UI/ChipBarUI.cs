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
    public List<RectTransform> chipButtonHolders = new List<RectTransform>();


    public static int selectedFolderIndex = 0;
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
            DestroyImmediate(Holder.gameObject);
        chipButtonHolders.Clear();

        FolderDropdown.options.Clear();

        foreach (var kv in FolderSystem.Enum)
            AddFolderView(kv.Value, kv.Key > 2 ? UserSprite : BuiltInSprite);

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
                holder = chipButtonHolders[0].transform;
                break;
            case ChipPackage.ChipType.Gate:
                holder = chipButtonHolders[1].transform;
                break;
            case ChipPackage.ChipType.Miscellaneous:
                holder = chipButtonHolders[2].transform;
                break;
            default:
                int index = 0;
                if (chip is CustomChip customChip)
                {
                    if (FolderSystem.ContainsIndex(customChip.FolderIndex))
                        index = customChip.FolderIndex;
                    else
                        customChip.FolderIndex = index;
                }

                holder = chipButtonHolders[index].transform;
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
            FolderDropdown.value = selectedFolderIndex;
            return;
        }


        selectedFolderIndex = FolderDropdown.value;

        for (int i = 0; i < chipButtonHolders.Count; i++)
            chipButtonHolders[i].gameObject.SetActive(i == selectedFolderIndex);

        scrollRect.content = chipButtonHolders[selectedFolderIndex];
        UpdateBarPos();
    }

    public void AddFolderView(string folderName, Sprite sprite)
    {
        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(folderName, sprite);

        FolderDropdown.options.Remove(newFolderOption);
        FolderDropdown.options.Add(newOption);
        FolderDropdown.options.Add(newFolderOption);
        NewChipButtonHolder(folderName);
    }

    void NewChipButtonHolder(string name)
    {
        RectTransform newHolder = Instantiate(chipButtonHolderPrefab).GetComponent<RectTransform>();
        newHolder.gameObject.name = name + "Chips";
        newHolder.gameObject.SetActive(false);
        chipButtonHolders.Add(newHolder);
        newHolder.SetParent(scrollRectViewport, false);
    }
}
