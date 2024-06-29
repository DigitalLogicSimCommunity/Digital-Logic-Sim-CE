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
        if (chipInstanceHolder == null|| wire == null)return;

        Pin sourcePin = wire.SourcePin;
        Pin targetPin = wire.TargetPin;

        parentChipIndex = chipInstanceHolder.ComponentChipIndex(sourcePin.chip);
        parentChipOutputIndex = sourcePin.index;

        childChipIndex = chipInstanceHolder.ComponentChipIndex(targetPin.chip);
        childChipInputIndex = targetPin.index;

        anchorPoints = wire.anchorPoints.ToArray();

        ColourThemeName = wire.GetComponentInChildren<WireDisplay>().CurrentTheme.Name;
    }
}