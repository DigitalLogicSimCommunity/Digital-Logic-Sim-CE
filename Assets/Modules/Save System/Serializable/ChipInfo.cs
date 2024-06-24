using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct ChipInfo
{
    public string name;
    public Color PackColor;
    public Color PackNameColor;
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