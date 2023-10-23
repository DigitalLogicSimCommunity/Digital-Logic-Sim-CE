using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;


public enum PinNameDisplayMode
{
    AltHover = 0,
    Hover = 1,
    AlwaysMain = 2,
    AlwaysAll = 3
}

public class ChipEditorOptions : MonoBehaviour
{
    public static ChipEditorOptions instance;

    public Slider scaleSlider;
    public TMP_Text displayPinNamesLabel;
    public Toggle showZoomHelperToggle;
    public Slider mouseWheelSensitivitySlider;
    public Slider camMoveSpeedSlider;

    public Action<PinNameDisplayMode> OnPinDisplayActionChange;
    [FormerlySerializedAs("activePinNameDisplayMode")] public PinNameDisplayMode ActiveMode;

    void Awake()
    {
        instance = this;
    }

    public void SetUIValues(ChipEditor editor)
    {
        OnDisplayPinNamesChanged(PlayerPrefs.GetInt("PinNameDisplayMode", 3));
        showZoomHelperToggle.SetIsOnWithoutNotify(
            PlayerPrefs.GetInt("ShowZoomHelper", 1) == 1);
        mouseWheelSensitivitySlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("MouseSensitivity", 0.1f));
        camMoveSpeedSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("CamMoveSpeed", 12f));
        scaleSlider.SetValueWithoutNotify(editor.Data.scale);
        ScalingManager.i.UpdateScale();
    }

    public void OnScaleChanged()
    {
        ScalingManager.i.SetScale(scaleSlider.value);
    }


    public void OnDisplayPinNamesChanged(int value)
    {
        ActiveMode = (PinNameDisplayMode)value;

        displayPinNamesLabel.text = ActiveMode switch
        {
            PinNameDisplayMode.AltHover => "Alt + Mouse Over",
            PinNameDisplayMode.Hover => "Mouse Over",
            PinNameDisplayMode.AlwaysMain => "Always Main",
            PinNameDisplayMode.AlwaysAll => "Always All",
            _ => displayPinNamesLabel.text
        };
        PlayerPrefs.SetInt("PinNameDisplayMode", value);
        OnPinDisplayActionChange?.Invoke(ActiveMode);
    }

    public void OnShowZoomHelperChanged()
    {
        ZoomManager.instance.showZoomHelper = showZoomHelperToggle.isOn;
        PlayerPrefs.SetInt("ShowZoomHelper", showZoomHelperToggle.isOn ? 1 : 0);
    }

    public void OnMouseWheelSensitivityChanged()
    {
        ZoomManager.instance.mouseWheelSensitivity =
            mouseWheelSensitivitySlider.value;
        PlayerPrefs.SetFloat("MouseSensitivity", mouseWheelSensitivitySlider.value);
    }

    public void OnCamMoveSpeedChanged()
    {
        ZoomManager.instance.camMoveSpeed = camMoveSpeedSlider.value;
        PlayerPrefs.SetFloat("CamMoveSpeed", camMoveSpeedSlider.value);
    }
}