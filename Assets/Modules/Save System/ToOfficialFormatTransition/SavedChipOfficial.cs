

using System;

[System.Serializable]
public class SavedChipOfficial
{
    public string Name;
    public UnityEngine.Color Colour;
    public SavedSignal InputSignal;
    public SavedSignal OutputSignal;
    public SavedChip SubChips;
    public SavedWire Connections;
    
}

[Serializable]
public class SavedSignal
{
    public string Name;
    public int ID;
    public double PositionY;
    public string ColourThemeName;
    public int GroupID;
    
}