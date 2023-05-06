using System;
using UnityEngine;

public class ChipEditor : MonoBehaviour
{
    public Transform chipImplementationHolder;
    public Transform wireHolder;

    public ChipInterfaceEditor inputsEditor;
    public ChipInterfaceEditor outputsEditor;
    public ChipInteraction chipInteraction;
    public PinAndWireInteraction pinAndWireInteraction;


    public ChipData Data;

    void Awake()
    {
        Data = new ChipData()
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

    public Chip LoadInstanceData(Chip chipData, Vector3 pos, Quaternion rot)
    {
        // Load component chips
        switch (chipData)
        {
            case InputSignal inp:
                inp.wireType = inp.outputPins[0].wireType;
                return inputsEditor.LoadSignal(inp, pos.y);
            case OutputSignal outp:
                outp.wireType = outp.inputPins[0].wireType;
                return outputsEditor.LoadSignal(outp, pos.y);
            default:
                return chipInteraction.LoadChip(chipData, pos);
        }
    }

    public Wire LoadWire(Pin connectedPin, Pin pin)
    {
        return pinAndWireInteraction.CreateAndLoadWire(connectedPin, pin);
    }
}