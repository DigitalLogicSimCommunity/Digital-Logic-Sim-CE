using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedWireLayout
{
    public SavedWire[] serializableWires;

    public SavedWireLayout(ChipInstanceHolder chipInstanceHolder)
    {
        if (chipInstanceHolder == null) return;
        Wire[] allWires = chipInstanceHolder.wires;
        serializableWires = new SavedWire[allWires.Length];


        for (int i = 0; i < allWires.Length; i++)
        {
            serializableWires[i] = new SavedWire(chipInstanceHolder, allWires[i]);
        }
    }
}