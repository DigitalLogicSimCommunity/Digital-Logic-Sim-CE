using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SavedWire
{
    public int parentChipIndex;
    public int parentChipOutputIndex;
    public int childChipIndex;
    public int childChipInputIndex;
    public Vector2[] anchorPoints;
    public string ColourThemeName;

    public SavedWire(ChipInstanceHolder chipInstanceHolder, Wire wire)
    {
        Pin parentPin = wire.startPin;
        Pin childPin = wire.endPin;

        parentChipIndex = chipInstanceHolder.ComponentChipIndex(parentPin.chip);
        parentChipOutputIndex = parentPin.index;

        childChipIndex = chipInstanceHolder.ComponentChipIndex(childPin.chip);
        childChipInputIndex = childPin.index;

        anchorPoints = wire.anchorPoints.ToArray();

        ColourThemeName = wire.GetComponentInChildren<WireDisplay>().GetCurrentThemeName();
    }
}