using System;
using DLS.ChipData;
using Newtonsoft.Json;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

[Serializable]
// Description of a chip: used for saving/loading data, and for setting up the simulation.
public class ChipDescription
{
    public string Version;

    // Name of the chip. This must be unique and not conflict with built-in chip names
    public string Name;

    // Colour to display this chip (e.g. "#FF0000")
    public string Colour;

    public int FolderIndex;

    public float Scale;


    // Description of this chip's input and output pins (such as their display names and other settings)
    public SignalDescription[] InputPins;
    public SignalDescription[] OutputPins;

    public string[] ChipDependencies;

    // Description of all sub-chips (chips contained within this chip).
    // The description contains their names and their names and positions within the chip.
    public ChipInstanceData[] SubChips;

    // Description of all connections (wires) between this chip and its subChips.
    public ConnectionDescription[] Connections;
}

[Serializable]
// Instance data for a SubChip (a chip contained within another chip).
public class ChipInstanceData
{
    // Name of the subchip: used to look up its full ChipDescription
    public string Name;

    // Unique ID: used to identify a particular subchip (since subchips of the same kind will share the same name)
    public int ID;

    // Position of the subchip inside its parent chip
    // (This is an array because some specialized chips, such as a Bus chip for example, may be defined by multiple points).
    public Point[] Points;

    // Array of arbitrary data that could be used by some specialized chips, such as a ROM chip for example.
    public byte[] Data;
}

[Serializable]
public class SignalDescription
{
    public string Name;
    public int ID;
    public int GroupID;
    public float PositionY;
    public string ColourThemeName;
    public Pin.WireType WireType;
}

[Serializable]
public class ConnectionDescription
{
    public PinAddress Source;
    public PinAddress Target;
    public Point[] WirePoints;
    public string ColourThemeName;
}

public enum PinType
{
    Unassigned,
    ChipInputPin,
    ChipOutputPin,
    SubChipInputPin,
    SubChipOutputPin
}

// Specifies a particular pin inside of a specific chip.
// This could be one of the chip’s own input/output pins, or an input/output pin of one of its direct subchips.
[Serializable]
public class PinAddress
{
    public PinType PinType;
    public int SubChipID;
    public int PinID;


    // Constructor
    public PinAddress()
    {
    }

    public PinAddress(int subChipID, int pinID, PinType pinType)
    {
        this.SubChipID = subChipID;
        this.PinType = pinType;
        this.PinID = pinID;
    }

    // ---- Helpers ----
    [JsonIgnore] public bool BelongsToSubChip => PinType is PinType.SubChipInputPin or PinType.SubChipOutputPin;

    [JsonIgnore] public bool IsInputPin => PinType is PinType.ChipInputPin or PinType.SubChipInputPin;

    public static bool AreSame(PinAddress a, PinAddress b)
    {
        return a.SubChipID == b.SubChipID && a.PinID == b.PinID && a.PinType == b.PinType;
    }

    public override string ToString()
    {
        return $"SubChipID: {SubChipID} PinID: {PinID} PinType: {PinType}";
    }
}