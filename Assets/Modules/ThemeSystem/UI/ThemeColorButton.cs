using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DLS.Module.ThemeSystem.UI
{
    public class ThemeColorButton : MonoBehaviour
    {
        //Event
        public event Action<ThemeColorButton> OnButtonMouseEnter;
        public event Action OnButtonMouseExit;
        public event Action<Palette.VoltageColour> OnClick;
        
        
        
        //Work
        private Palette.VoltageColour VoltageColour;
        private Button Button;
        private TMP_Text ButtonText;
        private ColorBlock NearColorBlock;
        private ColorBlock NormalColorBlock;

        private void Awake()
        {
            Button = GetComponent<Button>();
            ButtonText = GetComponentInChildren<TMP_Text>(true);
        }

        public void SetTheme(Palette.VoltageColour voltageColour)
        {
            VoltageColour = voltageColour;
            ButtonText.text = VoltageColour.Name;
            var OldColors = Button.colors;
            NearColorBlock = new ColorBlock()
            {
                highlightedColor = VoltageColour.GetHigh(Pin.WireType.Simple),
                normalColor = VoltageColour.Low,
                disabledColor = OldColors.disabledColor,
                fadeDuration = OldColors.fadeDuration,
                colorMultiplier = OldColors.colorMultiplier,
                pressedColor = OldColors.pressedColor,
                selectedColor = OldColors.selectedColor
            };
            NormalColorBlock = new ColorBlock()
            {
                highlightedColor = VoltageColour.GetHigh(Pin.WireType.Simple),
                normalColor = OldColors.normalColor,
                disabledColor = OldColors.disabledColor,
                fadeDuration = OldColors.fadeDuration,
                colorMultiplier = OldColors.colorMultiplier,
                pressedColor = VoltageColour.Low,
                selectedColor = OldColors.selectedColor
            };
            Button.colors = NormalColorBlock;
            Button.onClick.AddListener(ClickNotify);
        }
        


        public void SetAsNear()
        {
            Button.colors = NearColorBlock;
        }

        public void SetAsNormal()
        {
            Button.colors = NormalColorBlock;
        }

        public void MouseEnterNotify()
        {
            OnButtonMouseEnter?.Invoke(this);
        }

        public void MouseExitNotify()
        {
            OnButtonMouseExit?.Invoke();
        }

        private void ClickNotify()
        {
            OnClick?.Invoke(VoltageColour);
        }
    }
}