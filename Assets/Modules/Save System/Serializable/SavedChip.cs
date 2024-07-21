using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
///  Composite chip is a custom chip made up from other chips ("components")
/// </summary>
[System.Serializable]
public class SavedChip
{
    public string Version = "X.X.X";
    public ChipInfo Info;

    // Names of all chips used as components in this new chip (each name appear only once)
    public string[] ChipDependencies;
    // Data about all the chips used as components in this chip (positions,
    // connections, etc) Array is ordered: first come input signals, then output
    // signals, then remaining component chips
    public SavedComponentChip[] savedComponentChips;

    public SavedWire[] Connections;

    public SavedChip(ChipInstanceHolder chipInstanceHolder)
    {
        if (chipInstanceHolder == null)
            return;

        Version = GameConstant.GAMEVERSION_SAVE;

        Info = chipInstanceHolder.Info;

        // Create list of (unique) names of all chips used to make this chip
        ChipDependencies = chipInstanceHolder.componentChips.Select(x => x.Name)
                                .Distinct()
                                .SkipWhile(x=> x is "SIGNAL IN" or "SIGNAL OUT")
                                .ToArray();

        // Create serializable chips
        savedComponentChips = new SavedComponentChip[chipInstanceHolder.componentChips.Length];

        for (int i = 0; i < chipInstanceHolder.componentChips.Length; i++)
            savedComponentChips[i] = new SavedComponentChip(chipInstanceHolder, chipInstanceHolder.componentChips[i]);


        Wire[] allWires = chipInstanceHolder.wires;
        Connections = new SavedWire[allWires.Length];
        for (int i = 0; i < allWires.Length; i++)
            Connections[i] = new SavedWire(chipInstanceHolder, allWires[i]);

    }

    public void ValidateDefaultData()
    {
        Info.ValidateDefaultData();
    }


}
