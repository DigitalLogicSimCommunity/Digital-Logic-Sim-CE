using System;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public class ScalingManager : MonoBehaviour
{
    public static ScalingManager i;

    public event Action OnScaleChange;
    public float scale = 1f;
    public static float Scale => i.scale;

    [Header("handler")] const float maxhandleSizeY = 0.4f;
    const float minhandleSizeY = 0.1f;

    [SerializeField, Range(minhandleSizeY, maxhandleSizeY)]
    private float handleSizeY = 0.4f;

    public static float HandleSizeY => i.handleSizeY;


    const float maxFontSize = 1.75f;
    const float minFontSize = 0.3f;
    private float fontSize = 1.75f;
    public static float FontSize => i.fontSize;


    [Header("Pins")] const float maxPinSize = 0.4f;
    const float minPinSize = 0.1f;

    [SerializeField, Range(minPinSize, maxPinSize)]
    private float pinSize = 0.4f;

    public static float PinSize => i.pinSize;


    const float maxPinDisplayPadding = 0.1f;
    const float minPinDisplayPadding = 0.02f;

    [SerializeField, Range(minPinDisplayPadding, maxPinDisplayPadding)]
    private float pinDisplayPadding = 0.1f;

    public static float PinDisplayPadding => i.pinDisplayPadding;

    const float maxPinDisplayTextOffset = 0f;
    const float minPinDisplayTextOffset = -0.005f;

    [SerializeField, Range(minPinDisplayTextOffset, maxPinDisplayTextOffset)]
    private float pinDisplayTextOffset = 0f;

    public static float PinDisplayTextOffset => i.pinDisplayTextOffset;


    const float maxPinDisplayFontSize = 1.75f;
    const float minPinDisplayFontSize = 0.3f;

    [SerializeField, Range(minPinDisplayFontSize, maxPinDisplayFontSize)]
    private float pinDisplayFontSize = 1.75f;

    public static float PinDisplayFontSize => i.pinDisplayFontSize;

    [Header("Package")] const float maxPackageFontSize = 2.5f;
    const float minPackageFontSize = 0.5f;

    [SerializeField, Range(minPackageFontSize, maxPackageFontSize)]
    private float packageFontSize = 2.5f;

    public static float PackageFontSize => i.packageFontSize;

    const float maxChipInteractionBoundsBorder = 0.25f;
    const float minChipInteractionBoundsBorder = 0.05f;

    [SerializeField, Range(minChipInteractionBoundsBorder, maxChipInteractionBoundsBorder)]
    private float chipInteractionBoundsBorder = 0.25f;

    public static float ChipInteractionBoundsBorder => i.chipInteractionBoundsBorder;


    const float maxChipStackSpace = 0.15f;
    const float minChipStackSpace = 0.05f;

    [SerializeField, Range(minChipStackSpace, maxChipStackSpace)]
    private float chipStackSpace = 0.15f;

    public static float ChipStackSpace => i.chipStackSpace;

    [Header("wire")] const float maxWireThickness = 0.5f;
    const float minWireThickness = 0.1f;

    [SerializeField, Range(minWireThickness, maxWireThickness)]
    private float wireThickness = 0.5f;

    private float wireSelectedThickness = 0.5f;
    public static float WireThickness => i.wireThickness;
    public static float WireSelectedThickness => i.wireSelectedThickness * 1.5f;


    const float maxIOBarDistance = 8.15f;
    const float minIOBarDistance = 7.85f;
    [SerializeField, Range(minIOBarDistance, maxIOBarDistance)]
    private float ioBarDistance = 8.15f;
    public static float IoBarDistance => i.ioBarDistance;

    const float maxIOBarGraphicWidth = 1f;
    const float minIOBarGraphicWidth = 0.5f;
    [SerializeField, Range(minIOBarGraphicWidth, maxIOBarGraphicWidth)]
    private float ioBarGraphicWidth = 1f;
    public static float IoBarGraphicWidth => i.ioBarGraphicWidth;

    const float maxGroupSpacing = 0.22f;
    const float minGroupSpacing = 0.055f;
    [SerializeField, Range(minGroupSpacing, maxGroupSpacing)]
    private float groupSpacing = 0.22f;
    public static float GroupSpacing => i.groupSpacing;

    const float maxPropertiesUIX = 1.45f;
    const float minPropertiesUIX = 1.1f;
    [SerializeField, Range(minPropertiesUIX, maxPropertiesUIX)]
    private float propertiesUIX = 1.45f;
    public static float PropertiesUIX => i.propertiesUIX;

    const float maxPropertiesUIXZoom = 0.8f;
    const float minPropertiesUIXZoom = 0;
    [SerializeField, Range(minPropertiesUIXZoom, maxPropertiesUIXZoom)]
    private float propertiesUIXZoom = 0f;
    public static float PropertiesUIXZoom => i.propertiesUIXZoom;

    void Awake()
    {
        i = this;
    }

    private void Start()
    {
        Manager.instance.OnEditorClear += () => SetScale(1);
    }

    void Update()
    {
        propertiesUIXZoom = Mathf.Lerp(minPropertiesUIXZoom, maxPropertiesUIXZoom,
            ZoomManager.zoom);
        propertiesUIX = Mathf.Lerp(minPropertiesUIX, maxPropertiesUIX, scale) -
                        propertiesUIXZoom;
    }

    void CalcValues()
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

        handleSizeY = pinSize;
    }

    public void UpdateScale()
    {
        ChipEditor chipEditor = Manager.ActiveChipEditor;
        if (!chipEditor) return;

        CalcValues();

        OnScaleChange?.Invoke();
    }

    public void SetScale(float newScale)
    {
        scale = newScale;
        OnScaleChange?.Invoke();
    }

    private void OnValidate()
    {
        OnScaleChange?.Invoke();
    }
}