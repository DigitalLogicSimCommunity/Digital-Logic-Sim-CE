using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using SFB;
using System.Linq;

public class EditChipMenu : MonoBehaviour
{
    public TMP_InputField chipNameField;
    public Button doneButton;
    public Button deleteButton;
    public Button viewButton;
    public Button exportButton;
    public ChipBarUI chipBarUI;
    public TMP_Dropdown folderDropdown;
    private Chip currentChip;

    private string nameBeforeChanging;


    string CurrentTextValue { get => folderDropdown.options[folderDropdown.value].text; }

    void Awake()
    {
        chipNameField.onValueChanged.AddListener(ChipNameFieldChanged);
        doneButton.onClick.AddListener(FinishCreation);
        deleteButton.onClick.AddListener(SubmitDeleteChip);
        viewButton.onClick.AddListener(ViewChip);
        exportButton.onClick.AddListener(ExportChip);
    }

    public void EditChipInit(string chipName)
    {

        chipNameField.text = chipName;
        nameBeforeChanging = chipName;
        doneButton.interactable = true;
        chipNameField.interactable = ChipSaver.IsSafeToDelete(nameBeforeChanging);
        deleteButton.interactable = ChipSaver.IsSafeToDelete(nameBeforeChanging);

        currentChip = Manager.GetChipByName(chipName);
        viewButton.interactable = true;
        exportButton.interactable = true;

        folderDropdown.ClearOptions();
        var FolderOption = ChipBarUI.instance.FolderDropdown.options;
        folderDropdown.AddOptions(FolderOption.GetRange(1, FolderOption.Count - 2));


        if (currentChip is CustomChip customChip)
        {
            for (int i = 0; i < folderDropdown.options.Count; i++)
            {
                
                if (FolderSystem.CompareValue(customChip.FolderIndex, folderDropdown.options[i].text))
                {
                    folderDropdown.value = i;
                    break;
                }
            }
        }

    }

    public void ChipNameFieldChanged(string value)
    {
        string formattedName = value.ToUpper();
        doneButton.interactable = IsValidChipName(formattedName.Trim());
        chipNameField.text = formattedName;
    }


    public bool IsValidRename(string chipName)
    {
        // Name has not changed
        if (string.Equals(nameBeforeChanging, chipName))
            return true;
        // Name is either empty or in builtin chips
        if (!IsValidChipName(chipName))
            return false;

        SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
        for (int i = 0; i < savedChips.Length; i++)
        {
            // Name already exists in custom chips
            if (savedChips[i].Data.name == chipName)
                return false;
        }
        return true;
    }

    public bool IsValidChipName(string chipName)
    {
        // If chipName is not in list of builtin chips then is a valid name
        return !Manager.instance.AllChipNames(builtin: true, custom: false)
                    .Contains(chipName) && chipName.Length > 0;
    }

    public void SubmitDeleteChip()
    {
        UIManager.NewSubmitMenu(header: "Delete Chip",
                                text: "Are you sure you want to delete the chip '" +
                                    currentChip.chipName +
                                    "'?\nIt will be lost forever!",
                                onSubmit: DeleteChip);
    }

    public void DeleteChip()
    {
        ChipSaver.Delete(nameBeforeChanging);
        FindObjectOfType<ChipInteraction>().DeleteChip(currentChip);
        EditChipBar();
        DLSLogger.Log("Successfully deleted chip '" + currentChip.chipName + "'");
        currentChip = null;
    }

    public void EditChipBar()
    {
        Manager.instance.spawnableChips.Clear();
        SaveSystem.LoadAll(Manager.instance);
        chipBarUI.ReloadBar();
    }

    public void FinishCreation()
    {
        if (chipNameField.text != nameBeforeChanging)
        {
            // Chip has been renamed
            ChipSaver.Rename(nameBeforeChanging, chipNameField.text.Trim());
            EditChipBar();
        }
        if (currentChip is CustomChip customChip)
        {

            var index = FolderSystem.ReverseIndex(CurrentTextValue);
            if (index != customChip.FolderIndex)
            {
                ChipSaver.ChangeFolder(customChip.name, index);
                EditChipBar();
            }
        }
        currentChip = null;
    }

    public void ViewChip()
    {
        if (currentChip != null)
        {
            Manager.instance.ViewChip(currentChip);
            currentChip = null;
        }
    }

    public void ExportChip() { ImportExport.instance.ExportChip(currentChip); }
}
