using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockMenu : MonoBehaviour
{
    private Clock CurrentEditingClock;
    public TMP_InputField HzInputField;
    public Button doneButton;
    // Start is called before the first frame update
    void Start()
    {
        doneButton.onClick.AddListener(Done);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetClockToEdit (Clock Clock)
    {
        CurrentEditingClock = Clock;
        HzInputField.text = $"{Clock.Hz}Hz";
    }
    public void Done()
    {
        CurrentEditingClock.Hz= float.Parse(Regex.Match(HzInputField.text, @"\d+").Value);
    }
}
