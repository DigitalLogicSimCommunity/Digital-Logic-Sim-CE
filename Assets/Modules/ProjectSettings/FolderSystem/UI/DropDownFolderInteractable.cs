using System.Collections;
using System.Collections.Generic;
using Modules.ProjectSettings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DropDownFolderInteractable : MonoBehaviour
{

    public EditFolderMenu EditFolderMenu;

    private void OnMouseOver()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        var FolderName = name.Split(":")[1].Trim();
        if (ProjectSettings.FolderSystem.ReverseIndex(FolderName) <= 2) return;
        MenuManager.instance.OpenMenu(MenuType.EditFolderMenu);

        EditFolderMenu.SetFolderToBeRenamed(FolderName);

    }




}

