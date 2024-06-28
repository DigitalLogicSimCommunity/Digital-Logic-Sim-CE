using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LoadProjectMenu : MonoBehaviour
{
    public Button projectButtonPrefab;
    public Transform ScrollHolder;
    public ProjectRenamePopUp RenamePopUp;
    private List<Button> ProjectButtons = new List<Button>();



    public Button SelectedButton { get; private set; }
    public string SelectedProjectName => SelectedButton.GetComponentInChildren<TMPro.TMP_Text>().text;

    void Start()
    {
        SaveSystem.MigrateSaves();

        ReloadProjectList();
    }

    public void ReloadProjectList()
    {
        ClearProjectButtons();
        string[] projectNames = SaveSystem.GetProjectNames();

        for (int i = 0; i < projectNames.Length; i++)
        {
            string projectName = projectNames[i];
            if (i >= ProjectButtons.Count)
                ProjectButtons.Add(Instantiate(projectButtonPrefab, parent: ScrollHolder));
            Button loadButton = ProjectButtons[i];
            loadButton.GetComponentInChildren<TMPro.TMP_Text>().text =projectName.Trim();
            loadButton.onClick.AddListener(()=>
            {
                SelectedButton= loadButton;
                loadButton.Select();
            }) ;
        }
    }

    private void ClearProjectButtons()
    {
        foreach (var button in ProjectButtons)
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
        ProjectButtons.Clear();
    }

    public void LoadProject()
    {
        if(SelectedButton == null) return;
        SaveSystem.ActiveProjectName =SelectedProjectName ;
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }


    public void DeleteProject()
    {
        if(SelectedButton == null) return;
        SaveSystem.DeleteProject(SelectedProjectName);
        ReloadProjectList();
    }

    public void RenameProject()
    {
        if(SelectedButton == null) return;
        RenamePopUp.Active(false);
    }

    public void CopyProject()
    {
        if(SelectedButton == null) return;
        RenamePopUp.Active(true);
    }

    public void OpenProjectFolder()
    {
        EditorUtility.RevealInFinder(SaveSystem.SaveDataDirectoryPath);
    }

}