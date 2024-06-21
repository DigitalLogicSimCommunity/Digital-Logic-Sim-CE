using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateGroup : MonoBehaviour
{
    public static CreateGroup i;
    public event System.Action<int> onGroupSizeSettingPressed;

    public TMP_InputField groupSizeInput;
    public Button setSizeButton;
    public GameObject menuHolder;
    private bool menuActive;

    int groupSizeValue;

    private void Awake()
    {
        i = this;
    }

    void Start()
    {
        menuActive = false;
        groupSizeValue = 8;
        setSizeButton.onClick.AddListener(SetGroupSize);
        groupSizeInput.onValueChanged.AddListener(SetCurrentText);
    }

    void SetCurrentText(string groupSize)
    {
        if (groupSize != "" && groupSize != "-")
        {
            int result = int.Parse(groupSize);
            result = result <= 1 ? 1 : result;
            groupSizeValue = result > 16 ? 16 : result;
            groupSizeInput.SetTextWithoutNotify(groupSizeValue.ToString());
        }
        else if (groupSize == "-")
        {
            groupSizeInput.SetTextWithoutNotify("");
        }
    }

    public void CloseMenu()
    {
        onGroupSizeSettingPressed.Invoke(groupSizeValue);
        menuActive = false;
        menuHolder.SetActive(false);
    }

    public void OpenMenu()
    {
        menuActive = true;
        menuHolder.SetActive(true);
    }

    void SetGroupSize()
    {
        if (menuActive)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
}
