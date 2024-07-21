using System.Collections;
using System.Collections.Generic;
using Modules.ProjectSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewFolderMenu : MonoBehaviour
{
    ChipBarUI chipBarUI;
    public TMP_InputField newFolderNameField;
    public Button SubmitNewFolder;

    private void Start()
    {
        chipBarUI = ChipBarUI.instance;
    }

    public void NewFolder()
    {
        string newFolderName = newFolderNameField.text;

        newFolderNameField.SetTextWithoutNotify("");
        SubmitNewFolder.interactable = false;

        chipBarUI.AddFolderView(ProjectSettings.FolderSystem.AddFolder(newFolderName), chipBarUI.UserSprite);

    }



    public void CheckFolderName(bool endEdit = false)
    {
        var validName = FolderNameValidator.ValidateFolderName(newFolderNameField.text, endEdit);

        SubmitNewFolder.interactable = validName.Length > 0 && ProjectSettings.FolderSystem.FolderNameAvailable(validName);
        newFolderNameField.SetTextWithoutNotify(validName);
    }

}
