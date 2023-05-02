using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DLS.Simulation;

public class CustomChip : SpawnableChip
{

    public InputSignal[] inputSignals;
    public OutputSignal[] outputSignals;

    public int FolderIndex = 0;

    [HideInInspector]
    public List<Pin> unconnectedInputs = new List<Pin>();

    public override void Init()
    {
        Editable = true;
        ChipType = ChipType.Custom;
    }

    // Applies wire types from signals to pins
    public void ApplyWireModes()
    {
        foreach (var (pin, sig) in inputPins.Zip(inputSignals, (x, y) => (x, y)))
        {
            pin.wireType = sig.outputPins[0].wireType;
        }
        foreach (var (pin, sig)
                     in outputPins.Zip(outputSignals, (x, y) => (x, y)))
        {
            pin.wireType = sig.inputPins[0].wireType;
        }
    }

    protected override void ProcessOutput()
    {
        // Send signals from input pins through the chip
        for (int i = 0; i < inputPins.Count; i++)
        {
            inputSignals[i].SendSignal(inputPins[i].State);
        }
        foreach (Pin pin in unconnectedInputs)
        {
            pin.ReceiveSignal(PinStates.AllLow());
            pin.chip.ReceiveInputSignal(pin);
        }

        // Pass processed signals on to ouput pins
        for (int i = 0; i < outputPins.Count; i++)
        {
            outputPins[i].ReceiveSignal(outputSignals[i].inputPins[0].State);
        }
    }
}
