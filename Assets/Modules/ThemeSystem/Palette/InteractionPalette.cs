using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette/InteractionPalette")]
public class InteractionPalette : ScriptableObject
{
    [Header("Signal Handler")]
    public Color handleCol;
    public Color HighlightedHandleCol;
    public Color FocusedHandleCol;

    
    [Header("PIN")]
    public Color PinHighlighte;
    public Color PinDefaultColor;
    
    
    [Header("Wire")]
    public Color WireHighlighte;

}