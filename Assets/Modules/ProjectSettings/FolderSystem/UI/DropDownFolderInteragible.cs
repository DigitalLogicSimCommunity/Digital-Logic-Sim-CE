using System.Collections;
using System.Collections.Generic;
using Modules.ProjectSettings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropDownFolderInteragible : MonoBehaviour
{

    public UnityEvent<string> OnRightClick;

    public void RightClickHandler()
    {
        FindObjectOfType<EditFolderMenu>().name = gameObject.name.Split(":")[1].Trim();
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        var FolderName = name.Split(":")[1].Trim();
        if (ProjectSettings.FolderSystem.ReverseIndex(FolderName) <= 2) return;
        MenuManager.instance.OpenMenu(MenuType.EditFolderMenu);
        OnRightClick?.Invoke(FolderName);
    }




}

