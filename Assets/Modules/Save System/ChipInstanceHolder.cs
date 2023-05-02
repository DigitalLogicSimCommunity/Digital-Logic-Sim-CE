using System.Collections.Generic;
using UnityEngine;

public class ChipInstanceHolder
{

    public ChipData Data;

    // All chips used as components in this new chip (including input and output
    // signals)
    public Chip[] componentChips;
    // All wires in the chip (in case saving of wire layout is desired)
    public Wire[] wires;

    public ChipInstanceHolder() { }

    public ChipInstanceHolder(ChipEditor chipEditor)
    {
        List<Chip> componentChipList = new List<Chip>();

        var sortedInputs = chipEditor.inputsEditor.GetAllSignals();
        var sortedOutputs = chipEditor.outputsEditor.GetAllSignals();
        SortSignalsByYPosition(sortedInputs);
        SortSignalsByYPosition(sortedOutputs);

        componentChipList.AddRange(sortedInputs);
        componentChipList.AddRange(sortedOutputs);

        componentChipList.AddRange(chipEditor.chipInteraction.allChips);
        componentChips = componentChipList.ToArray();

        wires = chipEditor.pinAndWireInteraction.allWires.ToArray();
        Data = chipEditor.Data;
    }
    
    private void SortSignalsByYPosition(List<ChipSignal> signals)
    {
        signals.RemoveAll(x => x == null);

        signals.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
    }

    public int ComponentChipIndex(Chip componentChip)
    {
        return System.Array.IndexOf(componentChips, componentChip);
    }
}
