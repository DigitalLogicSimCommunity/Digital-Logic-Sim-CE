using System.Collections.Generic;
using System.Linq;
using Interaction.Display;
using UnityEngine;
using VitoBarra.Utils.TextVerifier;

namespace Interaction.Signal
{
    public class SignalInteractionPreview
    {
        private SignalInteraction PreviewSignal;

        public SignalInteractionPreview(SignalInteraction _previewSignal, Transform transform)
        {
            PreviewSignal = _previewSignal;
            PreviewSignal.transform.SetParent(transform);
            var SignalTransoform = PreviewSignal.transform.GetChild(1);
            SignalTransoform.GetChild(3).gameObject.SetActive(false);
            SignalTransoform.GetChild(4).gameObject.SetActive(false);
            PreviewSignal.gameObject.SetActive(false);
            ThemeManager.instance.OnDefaultThemeChange += UpdatePreviewColor;
        }

        public void UnregisterEvent()
        {
            ThemeManager.instance.OnDefaultThemeChange -= UpdatePreviewColor;
        }

        private void UpdatePreviewColor()
        {
            var signalDisplays = PreviewSignal.Signals.ChipSignals.Select(x => x.GetComponentInChildren<SignalDisplay>(true));

            foreach (var signalDisplay in signalDisplays)
            {
                signalDisplay.SetTheme(ThemeManager.DefaultTheme);
            }
        }

        public void SetParent(Transform newParent)
        {
                
        }
        

        public void Enable()
        {
            PreviewSignal.gameObject.SetActive(true);
        }

        public void Disable()
        {
            PreviewSignal.gameObject.SetActive(false);
        }

        public void AdjustYPosition()
        {
            PreviewSignal.MoveCenterYPosition(InputHelper.MouseWorldPos.y);
        }

        public void SetGroupSize(int desiredGroupSize)
        {
            foreach (var transform in PreviewSignal.SetGroupSize(desiredGroupSize).Select(x => x.ChipSignal.transform))
            {
                transform.GetChild(4).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);
            }


            UpdatePreviewColor();
            PreviewSignal.SetPinInteractable(false);
        }

        public void UpdatePositionWithScale(float positionX)
        {
            PreviewSignal.transform.SetXPos(positionX);
        }
    }
}