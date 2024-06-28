using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectRenamePopUp : MonoBehaviour
{
    public LoadProjectMenu projectLoadMenu;
    public TMPro.TMP_InputField RenameField;
    bool KeepOld = true;


    public void Active(bool keepOld)
    {
        KeepOld = keepOld;
        gameObject.SetActive(true);
        string iputPlaceHolder = projectLoadMenu.SelectedProjectName;
        if (KeepOld)
            iputPlaceHolder += " copy";
        RenameField.text = iputPlaceHolder;
    }

    private void Update()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.KeypadEnter, KeyCode.Return))
            ConfirmMove();
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape))
            Cancel();
    }

    public void ConfirmMove()
    {
        SaveSystem.MoveProject(projectLoadMenu.SelectedProjectName, RenameField.text, KeepOld);
        projectLoadMenu.ReloadProjectList();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}