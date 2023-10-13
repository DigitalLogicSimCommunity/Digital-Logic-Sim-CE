using System;
using UI.ThemeSystem;
using UnityEngine;

namespace DLS.UI.ThemeSystem
{
    public abstract class ThemeDisplay : MonoBehaviour, IThemeSettable
    {
        protected Palette.VoltageColour CurrentTheme;


        protected virtual void Start()
        {
            CurrentTheme = ThemeManager.Palette.GetDefaultTheme();
        }

        public string GetCurrentThemeName()
        {
            return CurrentTheme.Name;
        }

        public virtual void SetTheme(Palette.VoltageColour voltageColour)
        {
            CurrentTheme = voltageColour;
            ApplyTheme();
        }
        
        public virtual void SetTheme(string themeName)
        {
            CurrentTheme = ThemeManager.instance.GetTheme(themeName);
            ApplyTheme();
        }

        protected virtual void CheckedApplyTheme()
        {
            CurrentTheme ??= ThemeManager.DefaultTheme;
            ApplyTheme();
        }
        protected abstract void ApplyTheme();
    }
}