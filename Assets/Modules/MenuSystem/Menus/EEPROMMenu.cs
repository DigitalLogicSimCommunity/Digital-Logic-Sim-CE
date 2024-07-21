using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EEPROMMenu : MonoBehaviour
{
    private EEPROM CurrentEditingEEPROM;
    public Button loadBINButton;
    public Button saveBINButton;
    // Start is called before the first frame update
    void Start()
    {
        loadBINButton.onClick.AddListener(LoadBIN);
        saveBINButton.onClick.AddListener(SaveBIN);
    }

    public void SetEEPROMToEdit(EEPROM EEPROM)
    {
        CurrentEditingEEPROM = EEPROM;
    }
    public void SaveBIN()
    {
        if (CurrentEditingEEPROM == null) return;
        CurrentEditingEEPROM.DumpBinary();
    }
    public void LoadBIN()
    {
        if (CurrentEditingEEPROM == null) return;
        CurrentEditingEEPROM.OpenAndFlashBinary();
    }
}
