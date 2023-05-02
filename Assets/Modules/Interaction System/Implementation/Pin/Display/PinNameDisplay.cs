using TMPro;
using UnityEngine;

public class PinNameDisplay : MonoBehaviour
{

    public TMP_Text nameUI;
    public Transform background;
    public Vector2 backgroundPadding;

    public void Set(Pin pin)
    {

        nameUI.fontSize = ScalingManager.PinDisplayFontSize;

        nameUI.text = string.IsNullOrEmpty(pin.pinName) ? "UNNAMED PIN" : pin.pinName;

        backgroundPadding.x = ScalingManager.PinDisplayPadding;
        nameUI.rectTransform.localPosition =
            new Vector3(nameUI.rectTransform.localPosition.x,
                        ScalingManager.PinDisplayTextOffset,
                        nameUI.rectTransform.localPosition.z);

        float backgroundSizeX = nameUI.preferredWidth + backgroundPadding.x;
        float backgroundSizeY = nameUI.preferredHeight + backgroundPadding.y;
        background.localScale = new Vector3(backgroundSizeX, backgroundSizeY, 1);

        float spacingFromPin = (backgroundSizeX / 2 + PinDisplay.interactionRadius * 1.5f);
        spacingFromPin *= (pin.pinType == Pin.PinType.ChipInput) ? -1 : 1;
        transform.position = pin.transform.position +
                             Vector3.right * spacingFromPin + Vector3.forward * -1;
    }
}
