using System.Collections;
using System.Collections.Generic;
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
        if (Input.GetMouseButtonDown(1))
        {
            string FolderName = name.Split(":")[1].Trim();
            if (FolderSystem.ReverseIndex(FolderName) > 2)
            {
                MenuManager.instance.OpenMenu(MenuType.EditFolderMenu);
                OnRightClick?.Invoke(FolderName);
            }
        }
    }




}

