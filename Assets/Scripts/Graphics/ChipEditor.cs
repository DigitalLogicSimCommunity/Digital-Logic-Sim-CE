using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ChipEditor : MonoBehaviour
{
    public Transform chipImplementationHolder;
    public Transform wireHolder;

    public ChipInterfaceEditor inputsEditor;
    public ChipInterfaceEditor outputsEditor;
    public ChipInteraction chipInteraction;
    public PinAndWireInteraction pinAndWireInteraction;

    public List<InputSignal> InputSignals => inputsEditor.GetAllSignals().Cast<InputSignal>().ToList();
    public List<OutputSignal> OutputSignals => outputsEditor.GetAllSignals().Cast<OutputSignal>().ToList();


    public ChipInfo CurrentChip;

    void Awake()
    {
        CurrentChip = new ChipInfo()
        {
            FolderIndex = 0,
            scale = 1
        };


        pinAndWireInteraction.Init(chipInteraction, inputsEditor, outputsEditor);
        pinAndWireInteraction.onConnectionChanged += OnChipNetworkModified;
    }

    private void Start()
    {
        pinAndWireInteraction.RegisterEditorArea(GetComponentInChildren<PlacmentAreaEvent>(true).MouseInteraction);
    }

    void LateUpdate()
    {
        pinAndWireInteraction.OrderedUpdate();
        chipInteraction.OrderedUpdate();
    }

    void OnChipNetworkModified()
    {
        CycleDetector.MarkAllCycles(this);
    }

    public Chip LoadInstanceData(Chip chipData, SavedComponentChip subComponentDescriptor)
    {
        Vector2 pos = new Vector2(subComponentDescriptor.posX, subComponentDescriptor.posY);
        // Load component chips
        switch (chipData)
        {
            case ChipSignal signal:
            {
                Palette.VoltageColour theme = ThemeManager.instance.GetTheme(subComponentDescriptor.ThemeName);
                signal.GroupId = subComponentDescriptor.signalGroupId;
                switch (signal)
                {
                    case InputSignal input:
                        input.wireType = subComponentDescriptor.outputPins[0].wireType;
                        return inputsEditor.LoadSignal(input, pos.y,theme);
                    case OutputSignal output:
                        output.wireType = subComponentDescriptor.inputPins[0].wireType;
                        return outputsEditor.LoadSignal(output, pos.y,theme);
                    default:
                        return null;
                }
            }
            default:
                return chipInteraction.LoadChip(chipData, pos);
        }
    }

    public Wire LoadWire(Pin connectedPin, Pin pin)
    {
        return pinAndWireInteraction.CreateAndLoadWire(connectedPin, pin);
    }

    public void SetSignalCenter(Dictionary<int, float> signalGroupCenter)
    {
        inputsEditor.SetSignalCenter(signalGroupCenter);
        outputsEditor.SetSignalCenter(signalGroupCenter);
    }
}