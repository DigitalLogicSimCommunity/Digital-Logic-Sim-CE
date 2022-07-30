using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockMenu : MonoBehaviour
{
    private CultureInfo CulturalInfo;
    private Clock CurrentEditingClock;
    public TMP_InputField HzInputField;
    public Button doneButton;
    // Start is called before the first frame update
    void Start()
    {
        doneButton.onClick.AddListener(Done);
        HzInputField.onEndEdit.AddListener(FinishedEdit);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FinishedEdit(string str)
    {
        var HzStr = Regex.Match(str, @"^\d+([\.,]\d+)?").Value;
        HzInputField.text = (HzStr == "" ? CurrentEditingClock.Hz.ToString() : HzStr )+ "Hz";
    }
    public void SetClockToEdit(Clock Clock)
    {
        CurrentEditingClock = Clock;
        HzInputField.text = $"{Clock.Hz}Hz";
    }
    public void Done()
    {
        if (CurrentEditingClock == null) return;

        var HzStr = Regex.Match(HzInputField.text, @"^\d+([\.,]\d+)?").Value.Replace(",", ".");
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        CurrentEditingClock.Hz = float.Parse(HzStr, NumberStyles.Any, ci);
    }
}
