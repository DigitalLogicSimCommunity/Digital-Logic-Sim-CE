using System.Collections.Generic;
using System.Linq;
using Modules.ProjectSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Modules.ProjectSettings.ProjectSettings.FolderSystem;

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

    public Dictionary<string,CustomButton> ChipButtons = new();

    public Dictionary<int, (RectTransform Holder, int Value)> chipButtonHolders =new();


    public static int CurrentFolderIndex = 0;
    TMP_Dropdown.OptionData newFolderOption = new ("New Folder");

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
        foreach (var chipKeyValue in ChipButtons)
        {
            Destroy(chipKeyValue.Value.gameObject);
        }

        ChipButtons.Clear();

        foreach (var BuiltInChip in manager.SpawnableBuiltinChips)
            if (BuiltInChip is SpawnableChip spawnableChip)
                AddChipButton(spawnableChip);

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

        foreach (var kv in ProjectSettings.FolderSystem.Enum)
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


    void LateUpdate()
    {
        UpdateBarPos();
    }

    void UpdateBarPos()
    {
        float barPosY = (horizontalScroll.gameObject.activeSelf) ? 16 : 0;
        bar.localPosition = new Vector3(0, barPosY, 0);
    }

    public void DeactivateUnsafeToPalaceChip(string chipName)
    {
        var custom =Manager.instance.SpawnableCustomChips;
        foreach (var chip in custom)
        {
            if (chip.Name == chipName)
            {
                ChipButtons[chip.Name].interactable = false;
                continue;
            }

            if (chip is not CustomChip customChip) continue;

            if (customChip.IsDependentOn(chipName))
                ChipButtons[chip.Name].interactable = false;
        }
    }

    public void ActivateAllButton()
    {
        foreach (var chip in ChipButtons)
            chip.Value.interactable = true;
    }


    Transform GetFolderUI(SpawnableChip chip)
    {
        Transform holder1;
        switch (chip.ChipType)
        {
            case ChipType.Compatibility:
                holder1 = chipButtonHolders[(int)DefaultKays.Comp].Holder.transform;
                break;
            case ChipType.Gate:
                holder1 = chipButtonHolders[(int)DefaultKays.Gate].Holder.transform;
                break;
            case ChipType.Miscellaneous:
                holder1 = chipButtonHolders[(int)DefaultKays.Misc].Holder.transform;
                break;
            case ChipType.Custom:
            default:

                int index = (int)DefaultKays.Comp;
                if (chip is CustomChip customChip)
                {
                    if (ProjectSettings.FolderSystem.ContainsIndex(customChip.FolderIndex))
                        index = customChip.FolderIndex;
                }

                holder1 = chipButtonHolders[index].Holder.transform;
                break;
        }

        return holder1;
    }

    void AddChipButton(SpawnableChip chip)
    {
        if (hideList.Contains(chip.Name))
            return;

        CustomButton button = Instantiate(buttonPrefab);
        button.gameObject.name = "Create (" + chip.Name + ")";
        // Set button text
        var buttonTextUI = button.GetComponentInChildren<TMP_Text>();
        buttonTextUI.text = chip.Name;

        // Set button size
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta =
            new Vector2(buttonTextUI.preferredWidth + buttonWidthPadding,
                buttonRect.sizeDelta.y);


        // Set button position
        buttonRect.SetParent(GetFolderUI(chip), false);

        // Set button event
        button.AddListener(() => manager.ChipButtonHandler(chip));

        ChipButtons.Add(chip.Name,button);
    }

    void UpdateChipButton(Chip chip)
    {
        if (hideList.Contains(chip.Name))
            return;

        CustomButton button =ChipButtons[chip.Name];
        if (button == null) return;

        button.ClearEvents();
        button.AddListener(() => manager.ChipButtonHandler(chip));
    }

    public void SelectFolder()
    {
        if (FolderDropdown.value == FolderDropdown.options.Count - 1)
        {
            MenuManager.instance.OpenMenu(MenuType.NewFolderMenu);
            FolderDropdown.value = chipButtonHolders[CurrentFolderIndex].Value; // TODO set Last Used Folder
            return;
        }

        CurrentFolderIndex = ProjectSettings.FolderSystem.ReverseIndex(FolderDropdown.options[FolderDropdown.value].text);

        foreach (var chipHolder in chipButtonHolders)
            chipHolder.Value.Holder.gameObject.SetActive(false);

        var HolderSelecter = chipButtonHolders[CurrentFolderIndex].Holder;
        HolderSelecter.gameObject.SetActive(true);

        scrollRect.content = HolderSelecter;
        UpdateBarPos();
    }

    public void AddFolderView(int FolderIndex, Sprite sprite)
    {
        var folderName = ProjectSettings.FolderSystem.GetFolderName(FolderIndex);
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
        chipButtonHolders.Add(FolderIdex, (newHolder, FolderDropdown.options.Count - 2));
        newHolder.SetParent(scrollRectViewport, false);
    }
}