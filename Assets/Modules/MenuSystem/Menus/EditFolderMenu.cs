using System.Collections;
using System.Collections.Generic;
using Modules.ProjectSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditFolderMenu : MonoBehaviour
{
    ChipBarUI chipBarUI;
    public TMP_InputField RenamingFolderField;
    public TMP_Text RenamingTextLabel;
    public Button OKRenameFolder;
    private string FolderName = "";

    private void Start()
    {
        chipBarUI = ChipBarUI.instance;
    }

    public void RenameFolder()
    {
        string newFolderName = RenamingFolderField.text;
        RenamingFolderField.SetTextWithoutNotify("");
        OKRenameFolder.interactable = false;

        ProjectSettings.FolderSystem.RenameFolder(FolderName, newFolderName);
        chipBarUI.NotifyFolderNameChanged();
    }

    public void SubmitDeleteFolder()
    {
        MenuManager.NewSubmitMenu(header: "Delete Folder",
                        text: "Are you sure you want to delete the folder '" +
                            FolderName +
                            "'?\nIt will be lost forever!",
                        onSubmit: DeleteFolder);

    }

    public void DeleteFolder()
    {
        ProjectSettings.FolderSystem.DeleteFolder(FolderName);
        chipBarUI.NotifyRemovedFolder(FolderName);
    }

    public void InitMenu(string name) // call from Editor
    {
        FolderName = name;
        RenamingTextLabel.text = name;
        RenamingFolderField.Select();
    }

    public void CheckFolderName(bool endEdit = false)
    {
        var validName = FolderNameValidator.ValidateFolderName(RenamingFolderField.text, endEdit);

        OKRenameFolder.interactable = validName.Length > 0 && ProjectSettings.FolderSystem.FolderNameAvailable(validName);
        RenamingFolderField.SetTextWithoutNotify(validName);
    }
}




