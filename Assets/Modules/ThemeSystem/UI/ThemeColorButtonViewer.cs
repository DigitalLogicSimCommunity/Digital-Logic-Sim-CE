using System;
using System.Collections;
using System.Collections.Generic;
using UI.ThemeSystem;
using UnityEngine;

namespace DLS.Module.ThemeSystem.UI
{
    public class ThemeColorButtonViewer : MonoBehaviour
    {
        [SerializeField] private ThemeColorButton ButtonTemplate;
        public List<ThemeColorButton> ThemButtons;


        private void Start()
        {
            ThemButtons = new List<ThemeColorButton>();
            var menu = GetComponentInParent<ThemeChangerMenu>(true);
            foreach (var voltageColour in ThemeManager.Palette.voltageColours)
            {
                var button = Instantiate(ButtonTemplate, transform);
                button.SetTheme(voltageColour);
                button.OnButtonMouseEnter += SetUpColor;
                button.OnButtonMouseExit += SetAllNormal;
                button.OnClick += (x)=>
                {
                    SetAllNormal();
                    menu.ChangeTheme(x);
                };
                ThemButtons.Add(button);
            }
        }


        private void SetUpColor(ThemeColorButton button)
        {
            var index = ThemButtons.FindIndex(x => x == button);

            SetAllNormal();
            
            if (index+1 < ThemButtons.Count)
                ThemButtons[index + 1].SetAsNear();
            if (index > 0)
                ThemButtons[index - 1].SetAsNear();
        }

        private void SetAllNormal()
        {
            foreach (var butto in ThemButtons)
                butto.SetAsNormal();
        }
    }
}