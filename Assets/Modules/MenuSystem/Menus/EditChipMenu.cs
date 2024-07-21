using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using SFB;
using System.Linq;
using Modules.ProjectSettings;
using Modules.Save_System.Save;

public class EditChipMenu : MonoBehaviour
{
    public TMP_InputField chipNameField;
    public Button doneButton;
    public Button deleteButton;
    public Button viewButton;
    public Button exportButton;
    public TMP_Dropdown folderDropdown;
    private Chip currentChip;

    private string nameBeforeChanging;


    string CurrentFolderText { get => folderDropdown.options[folderDropdown.value].text; }

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
        var IsSafeToDelate = ChipSaver.IsChipSafeToDelete(nameBeforeChanging);
        chipNameField.interactable = true;
        deleteButton.interactable = IsSafeToDelate;

        currentChip = Manager.GetChipByName(chipName);
        viewButton.interactable = true;
        exportButton.interactable = true;

        folderDropdown.ClearOptions();
        var FolderOption = ChipBarUI.instance.FolderDropdown.options;
        folderDropdown.AddOptions(FolderOption.GetRange(1, FolderOption.Count - 2));


        if (currentChip is not CustomChip customChip) return;
        for (int i = 0; i < folderDropdown.options.Count; i++)
        {
            if (!ProjectSettings.FolderSystem.CompareValue(customChip.FolderIndex, folderDropdown.options[i].text)) continue;
            folderDropdown.value = i;
            break;
        }

    }

    private void ChipNameFieldChanged(string value)
    {
        string formattedName = value.ToUpper();
        doneButton.interactable = IsValidRename(formattedName.Trim());
        chipNameField.text = formattedName;
    }


    private bool IsValidRename(string chipName)
    {
        // Name has not changed
        if (string.Equals(nameBeforeChanging, chipName))
            return true;
        if (string.IsNullOrEmpty(chipName))
            return false;

        // chipName not present in either builtin chips nor custom chips
        return !IsChipNameBuiltIn(chipName) && !IsChipNameCustom(chipName);
    }

    private bool IsChipNameBuiltIn(string chipName)
    {
        return Manager.instance.AllChipNames(builtin: true, custom: false)
                    .Contains(chipName);
    }

    private bool IsChipNameCustom(string chipName)
    {
        return Manager.instance.AllChipNames(builtin: false, custom: true)
                    .Contains(chipName);
    }



    public void SubmitDeleteChip()
    {
        MenuManager.NewSubmitMenu(header: "Delete Chip",
                                text: $"Are you sure you want to delete the chip '{currentChip.Name}'? \nIt will be lost forever!",
                                onSubmit: DeleteChip);
    }

    public void DeleteChip()
    {
        ChipSaver.Delete(nameBeforeChanging);
        Manager.instance.DeleteChip(nameBeforeChanging);
        FindObjectOfType<ChipInteraction>().DeleteChip(currentChip);

        ReloadChipBar();


        DLSLogger.Log($"Successfully deleted chip '{currentChip.Name}'");
        currentChip = null;
    }

    public void ReloadChipBar()
    {
        ChipBarUI.instance.ReloadChipButton();
    }

    public void FinishCreation()
    {
        if (chipNameField.text != nameBeforeChanging)
        {
            // Chip has been renamed
            var NameAfterChanging = chipNameField.text.Trim();
            ChipSaver.Rename(nameBeforeChanging, NameAfterChanging);
            Manager.instance.RenameChip(nameBeforeChanging, NameAfterChanging);

            ReloadChipBar();
        }
        if (currentChip is CustomChip customChip)
        {

            var index = ProjectSettings.FolderSystem.ReverseIndex(CurrentFolderText);
            if (index != customChip.FolderIndex)
            {
                Manager.instance.ChangeFolderToChip(customChip.name, index);
                ReloadChipBar();
            }
        }
        currentChip = null;
    }

    public void ViewChip()
    {
        if (currentChip == null) return;
        
        Manager.instance.ViewChip(currentChip);
        currentChip = null;
    }

    public void ExportChip() { ImportExport.instance.ExportChip(currentChip); }
}
