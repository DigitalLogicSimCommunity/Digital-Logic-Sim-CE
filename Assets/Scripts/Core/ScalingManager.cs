using UnityEngine;

public class ScalingManager : MonoBehaviour
{
    public static ScalingManager instance;

    public static float scale = 1f;

    const float maxPinSize = 0.4f;
    const float minPinSize = 0.1f;
    public static float pinSize = 0.4f;
    public static float handleSizeY = 0.4f;

    const float maxFontSize = 1.75f;
    const float minFontSize = 0.3f;
    public static float fontSize = 1.75f;

    const float maxPinDisplayFontSize = 1.75f;
    const float minPinDisplayFontSize = 0.3f;
    public static float pinDisplayFontSize = 1.75f;

    const float maxPackageFontSize = 2.5f;
    const float minPackageFontSize = 0.5f;
    public static float packageFontSize = 2.5f;

    const float maxChipInteractionBoundsBorder = 0.25f;
    const float minChipInteractionBoundsBorder = 0.05f;
    public static float chipInteractionBoundsBorder = 0.25f;

    const float maxChipStackSpace = 0.15f;
    const float minChipStackSpace = 0.05f;
    public static float chipStackSpace = 0.15f;

    const float maxWireThickness = 0.5f;
    const float minWireThickness = 0.1f;
    public static float wireThickness = 0.5f;
    public static float wireSelectedThickness = 0.5f;

    const float maxPinDisplayPadding = 0.1f;
    const float minPinDisplayPadding = 0.02f;
    public static float pinDisplayPadding = 0.1f;

    const float maxPinDisplayTextOffset = 0f;
    const float minPinDisplayTextOffset = -0.005f;
    public static float pinDisplayTextOffset = 0f;

    const float maxIOBarDistance = 8.15f;
    const float minIOBarDistance = 7.85f;
    public static float ioBarDistance = 8.15f;

    const float maxIOBarGraphicWidth = 1f;
    const float minIOBarGraphicWidth = 0.5f;
    public static float ioBarGraphicWidth = 1f;

    const float maxGroupSpacing = 0.22f;
    const float minGroupSpacing = 0.055f;
    public static float groupSpacing = 0.22f;

    const float maxPropertiesUIX = 1.45f;
    const float minPropertiesUIX = 1.1f;
    public static float propertiesUIX = 1.45f;

    const float maxPropertiesUIXZoom = 0.8f;
    const float minPropertiesUIXZoom = 0;
    float propertiesUIXZoom = 0f;

    void Awake() { instance = this; }

    void Update()
    {
        propertiesUIXZoom = Mathf.Lerp(minPropertiesUIXZoom, maxPropertiesUIXZoom,
                                       ZoomManager.zoom);
        propertiesUIX = Mathf.Lerp(minPropertiesUIX, maxPropertiesUIX, scale) -
                        propertiesUIXZoom;
    }

    static void CalcValues()
    {
        scale = Mathf.Clamp01(scale);

        pinSize = Mathf.Lerp(minPinSize, maxPinSize, scale);
        fontSize = Mathf.Lerp(minFontSize, maxFontSize, scale);
        chipInteractionBoundsBorder = Mathf.Lerp(
            minChipInteractionBoundsBorder, maxChipInteractionBoundsBorder, scale);
        chipStackSpace = Mathf.Lerp(minChipStackSpace, maxChipStackSpace, scale);
        pinDisplayPadding =
            Mathf.Lerp(minPinDisplayPadding, maxPinDisplayPadding, scale);
        pinDisplayTextOffset =
            Mathf.Lerp(minPinDisplayTextOffset, maxPinDisplayTextOffset, scale);
        ioBarDistance = Mathf.Lerp(minIOBarDistance, maxIOBarDistance, scale);
        ioBarGraphicWidth =
            Mathf.Lerp(minIOBarGraphicWidth, maxIOBarGraphicWidth, scale);
        groupSpacing = Mathf.Lerp(minGroupSpacing, maxGroupSpacing, scale);

        pinDisplayFontSize =
            Mathf.Clamp(fontSize, minPinDisplayFontSize, maxPinDisplayFontSize);
        packageFontSize =
            Mathf.Clamp(fontSize * 1.5f, minPackageFontSize, maxPackageFontSize);
        wireThickness =
            Mathf.Clamp(pinSize * 1.5f, minWireThickness, maxWireThickness);
        wireSelectedThickness = wireThickness * 1.5f;

        handleSizeY = pinSize;
    }

    public static void UpdateScale()
    {
        ChipEditor chipEditor = Manager.ActiveChipEditor;
        if (chipEditor)
        {
            CalcValues();

            chipEditor.UpdateChipSizes();
            chipEditor.pinNameDisplayManager.UpdateTextSize(pinDisplayFontSize);
            chipEditor.inputsEditor.UpdateScale();
            chipEditor.outputsEditor.UpdateScale();

            foreach (Chip chip in chipEditor.chipInteraction.allChips)
            {
                foreach (Pin pin in chip.inputPins)
                {
                    pin.SetScale();
                }
                foreach (Pin pin in chip.outputPins)
                {
                    pin.SetScale();
                }
            }
            foreach (IOScaler scaler in FindObjectsOfType<IOScaler>())
            {
                scaler.UpdateScale();
            }
        }
    }
}
