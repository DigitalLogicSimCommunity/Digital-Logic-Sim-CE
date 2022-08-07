using System.Collections;
using System.Collections.Generic;
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

        FolderSystem.AddFolder(newFolderName);
        chipBarUI.AddFolderView(newFolderName, chipBarUI.UserSprite);

    }



    public void CheckFolderName(bool endEdit = false)
    {
        var validName = FolderNameValidator.ValidateFolderName(newFolderNameField.text, endEdit);

        SubmitNewFolder.interactable = validName.Length > 0 && FolderSystem.FolderNameAvailable(validName);
        newFolderNameField.SetTextWithoutNotify(validName);
    }

}
