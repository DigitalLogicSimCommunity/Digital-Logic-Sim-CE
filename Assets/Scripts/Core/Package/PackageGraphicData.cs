using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core
{
    [Serializable]
    public class PackageGraphicData
    {
        public Color PackageColour;
        [FormerlySerializedAs("TextColor")] public Color NameTextColor = Color.white;

        public bool OverrideWidthAndHeight = false;
        public float Width = 1f;
        public float Height = 1f;
    }
}