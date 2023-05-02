using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PinNameDisplayManager : MonoBehaviour
{
    public PinNameDisplay pinNamePrefab;
    ChipEditor chipEditor;
    ChipEditorOptions editorDisplayOptions;
    Pin highlightedPin;

    [HideInInspector] public List<PinNameDisplay> pinNameDisplays;
    List<Pin> pinsToDisplay;

    void Awake()
    {
        chipEditor = FindObjectOfType<ChipEditor>();
        editorDisplayOptions = FindObjectOfType<ChipEditorOptions>();
        // chipEditor.pinAndWireInteraction.onMouseOverPin += OnMouseOverPin;
        // chipEditor.pinAndWireInteraction.onMouseExitPin += OnMouseExitPin;

        pinNameDisplays = new List<PinNameDisplay>();
        pinsToDisplay = new List<Pin>();
    }

    public void UpdateTextSize(float fontSize)
    {
        foreach (PinNameDisplay display in pinNameDisplays)
            display.nameUI.fontSize = fontSize;
    }

    void LateUpdate()
    {
        var mode = editorDisplayOptions.activePinNameDisplayMode;
        pinsToDisplay.Clear();

        void SelectPinToDisplay(ChipInterfaceEditor chipInterface)
        {
            foreach (var chip in chipInterface.GetAllSignals().Where(chip => chipInterface.selectedSignals != null && !chipInterface.selectedSignals.Contains(chip)))
            {
                pinsToDisplay.AddRange(chip.inputPins);
            }
        }

        if (mode is ChipEditorOptions.PinNameDisplayMode.AlwaysMain or ChipEditorOptions.PinNameDisplayMode.AlwaysAll)
        {
            SelectPinToDisplay(chipEditor.inputsEditor);
            SelectPinToDisplay(chipEditor.outputsEditor);
        }

        if (highlightedPin)
        {
            bool nameDisplayKey =
                InputHelper.AnyOfTheseKeysHeld(KeyCode.LeftAlt, KeyCode.RightAlt);
            if (nameDisplayKey ||
                mode == ChipEditorOptions.PinNameDisplayMode.Hover)
            {
                pinsToDisplay.Add(highlightedPin);
            }
        }

        DisplayPinName(pinsToDisplay);
    }

    public void DisplayPinName(List<Pin> pins)
    {
        if (pinNameDisplays.Count < pins.Count)
        {
            int numToAdd = pins.Count - pinNameDisplays.Count;
            for (int i = 0; i < numToAdd; i++)
            {
                pinNameDisplays.Add(Instantiate(pinNamePrefab, parent: transform));
            }
        }
        else if (pinNameDisplays.Count > pins.Count)
        {
            for (int i = pins.Count; i < pinNameDisplays.Count; i++)
            {
                pinNameDisplays[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < pins.Count; i++)
        {
            pinNameDisplays[i].gameObject.SetActive(true);
            pinNameDisplays[i].Set(pins[i]);
        }
    }
    
    void OnMouseOverPin(Pin pin)
    {
        highlightedPin = pin;
    }

    void OnMouseExitPin(Pin pin)
    {
        if (highlightedPin == pin)
        {
            highlightedPin = null;
        }
    }
}