using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DecimalDisplay : MonoBehaviour
{
    private TMP_Text text;


    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }


    public void UpdateDecimalDisplay(IList<ChipSignal> signals, bool useTwosComplement)
    {
        int decimalValue = 0;
        for (int i = 0; i < signals.Count; i++)
        {
            var signalState = signals[signals.Count - 1 - i].State[0];
            if (useTwosComplement && i == signals.Count - 1)
                decimalValue |= (-(signalState.Toint() << i));
            else
                decimalValue |= (signalState.Toint() << i);
        }

        if (!text)
            text = GetComponent<TMP_Text>();
        text.text = decimalValue + "";
    }
}