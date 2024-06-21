using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ChipData
{
    public string name;
    public int creationIndex;
    public Color Colour;
    public Color NameColour;
    public int FolderIndex;
    public float scale;

    public void ValidateDefaultData()
    {
        if (float.IsNaN(FolderIndex))
            FolderIndex = 0;
        if (float.IsNaN(scale))
            scale = 1f;
    }
}