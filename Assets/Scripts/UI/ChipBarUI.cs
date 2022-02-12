using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChipBarUI : MonoBehaviour {
    public static ChipBarUI instance;
    public RectTransform bar;
    public GameObject chipButtonHolderPrefab;
    public CustomButton buttonPrefab;
    public float buttonSpacing = 15f;
    public float buttonWidthPadding = 10;
    float rightmostButtonEdgeX;
    Manager manager;
    public List<string> hideList;
    public Scrollbar horizontalScroll;
    public ScrollRect scrollRect;
    public Transform scrollRectViewport;

    public TMP_Dropdown selectedFolderDropdown;
    public TMP_InputField newFolderNameField;
    public Button SubmitNewFolder;

    public Sprite newFolderIcon;

    public List<CustomButton> customButton = new List<CustomButton>();
    public List<RectTransform> chipButtonHolders = new List<RectTransform>();

    public static int selectedFolderIndex = 0;
    string validChars =
        "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]";

    public static Dictionary<string, int> folders =
    new Dictionary<string, int>() { { "Basic", 0 },
        { "Advanced", 1 },
        { "User", 2 }
    };

    TMP_Dropdown.OptionData newFolderOption =
        new TMP_Dropdown.OptionData("New Folder");

    void Awake() {
        instance = this;
        manager = FindObjectOfType<Manager>();
        manager.customChipCreated += AddChipButton;
        manager.customChipUpdated += UpdateChipButton;

        newFolderOption.image = newFolderIcon;
        selectedFolderDropdown.AddOptions(
            new List<TMP_Dropdown.OptionData> { newFolderOption });

        Dictionary<string, int> customFolders = SaveSystem.LoadCustomFolders();
        foreach (KeyValuePair<string, int> kv in customFolders) {
            if (kv.Value > 2) {
                folders[kv.Key] = kv.Value;
                AddFolder(kv.Key);
            }
        }
        ReloadBar();
    }

    public void ReloadBar() {
        foreach (CustomButton button in customButton) {
            Destroy(button.gameObject);
        }
        customButton.Clear();
        for (int i = 0; i < manager.builtinChips.Length; i++) {
            AddChipButton(manager.builtinChips[i]);
        }
        Canvas.ForceUpdateCanvases();
    }

    void LateUpdate() {
        UpdateBarPos();
    }

    void UpdateBarPos() {
        float barPosY = (horizontalScroll.gameObject.activeSelf) ? 16 : 0;
        bar.localPosition = new Vector3(0, barPosY, 0);
    }

    void AddChipButton(Chip chip) {

        if (hideList.Contains(chip.chipName)) {
            return;
        }
        ChipPackage package = chip.GetComponent<ChipPackage>();
        Transform holder;
        switch (package.chipType) {
        case ChipPackage.ChipType.Basic:
            holder = chipButtonHolders[0].transform;
            break;
        case ChipPackage.ChipType.Advanced:
            holder = chipButtonHolders[1].transform;
            break;
        default:
            int index = 2;
            if (chip is CustomChip customChip) {
                if (folders.ContainsKey(customChip.folderName)) {
                    index = folders[customChip.folderName];
                }
            }
            holder = chipButtonHolders[index].transform;
            // Debug.Log("Add Chip '" + chip.chipName + "' to '" + holder.name + "'");
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
        // buttonRect.localPosition = new Vector3 (rightmostButtonEdgeX +
        // buttonSpacing + buttonRect.sizeDelta.x / 2f, 0, 0);
        rightmostButtonEdgeX =
            buttonRect.localPosition.x + buttonRect.sizeDelta.x / 2f;

        // Set button event
        // button.onClick.AddListener (() => manager.SpawnChip (chip));
        button.AddListener(() => manager.SpawnChip(chip));

        customButton.Add(button);
    }

    void UpdateChipButton(Chip chip) {
        if (hideList.Contains(chip.chipName)) {
            return;
        }

        CustomButton button =
            customButton.Find(g => g.name == "Create (" + chip.chipName + ")");
        if (button != null) {
            button.ClearEvents();
            button.AddListener(() => manager.SpawnChip(chip));
        }
    }

    public void SelectFolder() {
        if (selectedFolderDropdown.value ==
                selectedFolderDropdown.options.Count - 1) {
            UIManager.instance.OpenMenu(MenuType.NewFolderMenu);
            selectedFolderDropdown.value = selectedFolderIndex;
            return;
        }

        selectedFolderIndex = selectedFolderDropdown.value;

        for (int i = 0; i < chipButtonHolders.Count; i++) {
            chipButtonHolders[i].gameObject.SetActive(i == selectedFolderIndex);
        }
        scrollRect.content = chipButtonHolders[selectedFolderIndex];
        UpdateBarPos();
    }

    public void CheckFolderName(bool endEdit = false) {
        string validName = "";
        string text = newFolderNameField.text;
        for (int i = 0; i < text.Length; i++) {
            if (i < 12 && validChars.Contains(text[i].ToString())) {
                validName += text[i];
            }
        }
        validName = endEdit ? validName.Trim() : validName.TrimStart();

        SubmitNewFolder.interactable =
            validName.Length > 0 && FolderNameAvailable(validName);
        newFolderNameField.SetTextWithoutNotify(validName);
    }

    public void NewFolder() {
        string newFolderName = newFolderNameField.text;

        newFolderNameField.SetTextWithoutNotify("");
        SubmitNewFolder.interactable = false;

        folders[newFolderName] = folders.Count;
        SaveSystem.SaveCustomFolders(folders);

        AddFolder(newFolderName);
    }

    void AddFolder(string folderName) {
        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(
            folderName, selectedFolderDropdown.options[2].image);
        selectedFolderDropdown.options.Remove(newFolderOption);
        selectedFolderDropdown.options.Add(newOption);
        selectedFolderDropdown.options.Add(newFolderOption);
        NewChipButtonHolder(folderName);
    }

    bool FolderNameAvailable(string name) {
        foreach (string f in folders.Keys) {
            if (name.ToUpper() == f.ToUpper()) {
                return false;
            }
        }
        return true;
    }

    void NewChipButtonHolder(string name) {
        RectTransform newHolder =
            Instantiate(chipButtonHolderPrefab).GetComponent<RectTransform>();
        newHolder.gameObject.name = name + "Chips";
        newHolder.gameObject.SetActive(false);
        chipButtonHolders.Add(newHolder);
        newHolder.SetParent(scrollRectViewport, false);
    }
}
