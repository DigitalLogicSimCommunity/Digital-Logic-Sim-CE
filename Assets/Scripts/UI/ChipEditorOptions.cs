using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChipEditorOptions : MonoBehaviour
{
    public static ChipEditorOptions instance;

    public enum PinNameDisplayMode
    {
        AltHover = 0,
        Hover = 1,
        AlwaysMain = 2,
        AlwaysAll = 3
    }

    public PinNameDisplayMode activePinNameDisplayMode;

    public Slider scaleSlider;
    public TMP_Text displayPinNamesLabel;
    public Toggle showZoomHelperToggle;
    public Slider mouseWheelSensitivitySlider;
    public Slider camMoveSpeedSlider;

    void Awake() { instance = this; }

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
        ScalingManager.i.SetScale( scaleSlider.value);
        ScalingManager.i.UpdateScale();
    }

    public void OnDisplayPinNamesChanged(int value)
    {
        switch (value)
        {
            case 0:
                activePinNameDisplayMode = PinNameDisplayMode.AltHover;
                displayPinNamesLabel.text = "Alt + Mouse Over";
                break;
            case 1:
                activePinNameDisplayMode = PinNameDisplayMode.Hover;
                displayPinNamesLabel.text = "Mouse Over";
                break;
            case 2:
                activePinNameDisplayMode = PinNameDisplayMode.AlwaysMain;
                displayPinNamesLabel.text = "Always Main";
                break;
            case 3:
                activePinNameDisplayMode = PinNameDisplayMode.AlwaysAll;
                displayPinNamesLabel.text = "Always All";
                break;
        }
        PlayerPrefs.SetInt("PinNameDisplayMode", value);
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
