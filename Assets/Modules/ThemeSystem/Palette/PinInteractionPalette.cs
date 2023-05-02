using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette/PinInteractionPalette")]
public class PinInteractionPalette : ScriptableObject
{
    [Header("Signal Handler")]
    public Color handleCol;
    [FormerlySerializedAs("highlightedHandleCol")] public Color HighlightedHandleCol;
    public Color FocusedHandleCol;

    
    [Header("PIN")]
    public Color PinHighlighte;
    [FormerlySerializedAs("Pindefault")] public Color PinDefaultColor;
    
    
    [Header("Wire")]
    public Color WireHighlighte;

}