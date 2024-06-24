using System;
using System.Diagnostics.Tracing;
using Interaction.Signal;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VitoBarra.Utils.TextVerifier;

public class PinNameDisplay : MonoBehaviour
{
    public TMP_Text nameUI;
    public Transform background;
    public Vector2 backgroundPadding;
    private PinNameDisplayMode Mode;
    private Pin pin;
    private PinEvent PinEvent;
    private SignalInteraction Interaction;

    private bool IsInteraction = false;

    private bool ShouldBeDisplayed => Mode == PinNameDisplayMode.AlwaysAll ||
                                     (Mode == PinNameDisplayMode.AlwaysMain && IsInteraction);

    private void Awake()
    {
        var interaction = GetComponentInParent<SignalInteraction>();
        if (interaction)
        {
            Interaction =interaction;
            IsInteraction = true;
            Interaction.OnPropertyChange += NameChanged;
        }

        pin = GetComponentInParent<Pin>();

        PinEvent = transform.parent.GetComponentInChildren<PinEvent>();
        PinEvent.MouseInteraction.MouseExitted += MouseExitHandler;
        PinEvent.OnMOuseOver += OverHandler;


        ScalingManager.i.OnScaleChange += UpdateScale;
        SetMode( ChipEditorOptions.instance.ActiveMode);
    }


    private void MouseExitHandler(Pin obj)
    {
        if (!ShouldBeDisplayed)
            HideDisplay();
    }

    private void OverHandler()
    {
        var b = Mode == PinNameDisplayMode.Hover;
        if (b || Keyboard.current.altKey.isPressed)
            EnableDisplay();
        else if (!ShouldBeDisplayed)
            HideDisplay();
    }

    //cambiare il metodo d accesso
    private void Start()
    {
        UpdateScale();
        ChipEditorOptions.instance.OnPinDisplayActionChange += SetMode;
    }

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= UpdateScale;
        ChipEditorOptions.instance.OnPinDisplayActionChange -= SetMode;
        if (IsInteraction)
            Interaction.OnPropertyChange -= NameChanged;
    }

    private void SetMode(PinNameDisplayMode newMode)
    {
        Mode = newMode;
        switch (Mode)
        {
            case PinNameDisplayMode.AltHover:
            case PinNameDisplayMode.Hover:
                HideDisplay();
                break;
            case PinNameDisplayMode.AlwaysMain:
                if (IsInteraction)
                    EnableDisplay();
                break;
            case PinNameDisplayMode.AlwaysAll:
                EnableDisplay();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void EnableDisplay()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void HideDisplay(Pin obj = null)
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }


    private void NameChanged()
    {
        nameUI.text = string.IsNullOrEmpty(pin.pinName) ? "UNNAMED PIN" : pin.pinName;
        UpdateScale();
    }

    private void UpdateScale()
    {
        nameUI.fontSize = ScalingManager.PinDisplayFontSize;

        backgroundPadding.x = ScalingManager.PinDisplayPadding;

        nameUI.rectTransform.localPosition = new Vector3(nameUI.rectTransform.localPosition.x,
            ScalingManager.PinDisplayTextOffset, nameUI.rectTransform.localPosition.z);

        float backgroundSizeX = nameUI.preferredWidth + backgroundPadding.x;
        float backgroundSizeY = nameUI.preferredHeight + backgroundPadding.y;
        background.localScale = new Vector3(backgroundSizeX, backgroundSizeY, 1);

        float spacingFromPin = (backgroundSizeX / 2 + PinDisplay.interactionRadius + 0.5f);
        if (!IsInteraction)
            spacingFromPin *= (pin.pinType == Pin.PinType.ChipInput ? -1 : 1);

        transform.SetXLocalPos(spacingFromPin);
    }

    private void OnValidate()
    {
        if (ScalingManager.i != null)
            UpdateScale();
    }
}