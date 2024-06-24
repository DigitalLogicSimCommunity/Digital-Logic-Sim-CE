using System;
using UI.ThemeSystem;
using UnityEngine;

namespace DLS.UI.ThemeSystem
{
    public abstract class ThemeDisplay : MonoBehaviour, IThemeSettable
    {
        private Palette.VoltageColour _currentTheme;
        public Palette.VoltageColour CurrentTheme
        {
            get
            {
                return _currentTheme ??= ThemeManager.Palette.GetDefaultTheme();
            }
            set
            {
                _currentTheme = value;
                ApplyTheme();
            }
        }


        public virtual void SetThemeByName(string themeName)
        {
            CurrentTheme = ThemeManager.instance.GetTheme(themeName);
        }

        protected abstract void ApplyTheme();
    }
}