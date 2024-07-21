using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipInstanceHolder
{

    public ChipInfo Info;

    // All chips used as components in this new chip (including input and output signals)
    public Chip[] componentChips;
    // All wires in the chip (in case saving of wire layout is desired)
    public Wire[] wires;

    public ChipInstanceHolder() { }

    public ChipInstanceHolder(ChipEditor chipEditor)
    {
        List<Chip> componentChipList = new List<Chip>();

        var sortedInputs = chipEditor.InputSignals;
        sortedInputs.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        var sortedOutputs = chipEditor.OutputSignals;
        sortedInputs.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        componentChipList.AddRange(sortedInputs);
        componentChipList.AddRange(sortedOutputs);

        componentChipList.AddRange(chipEditor.chipInteraction.allChips);
        componentChips = componentChipList.ToArray();

        wires = chipEditor.pinAndWireInteraction.allWires.ToArray();
        Info = chipEditor.CurrentChip;
    }
    


    public int ComponentChipIndex(Chip componentChip)
    {
        return System.Array.IndexOf(componentChips, componentChip);
    }
}
