using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DLS.Core.Simulation;
using UnityEngine.Serialization;

public class CustomChip : SpawnableChip
{
    public bool AnyUnconnectedPin =>InternalUnconnectedInput.Length != 0;

    public InputSignal[] inputSignals;
    public OutputSignal[] outputSignals;

    public int FolderIndex = 0;

    public Pin[] InternalUnconnectedInput;
    public SpawnableChip[] InternalChipNoInput;

    public List<string> FullDependencies = new ();

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

    public override void ProcessOutput()
    {
        // Send signals from input pins through the chip
        for (int i = 0; i < inputPins.Count; i++)
        {
            inputSignals[i].SendSignal(inputPins[i].State);
        }
        foreach (Pin pin in InternalUnconnectedInput)
        {
            pin.ReceiveSignal(PinStates.AllLow());
            pin.chip.ReceiveInputSignal(pin);
        }
        foreach (SpawnableChip chip in InternalChipNoInput)
        {
            chip.ProcessOutput();
        }

        // Pass processed signals on to ouput pins
        for (int i = 0; i < outputPins.Count; i++)
        {
            outputPins[i].ReceiveSignal(outputSignals[i].inputPins[0].State);
        }
    }

    public bool IsDependentOn(string chipName)
    {
        return FullDependencies.Contains(chipName);
    }
}
