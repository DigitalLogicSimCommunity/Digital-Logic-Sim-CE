
using Interaction.Display;
using UI.ThemeSystem;
using UnityEngine;
using VitoBarra.Utils.TextVerifier;

public class ThemeChangerMenu : MonoBehaviour
{
    private RectTransform ThemeChangerMenuUI;
    private IThemeSettable ItemToSet;
    private bool Inizialized = false;

    private void Awake()
    {
        ThemeChangerMenuUI = (RectTransform)transform.GetChild(0);
    }


    public void OpenUI(IThemeSettable itemToSet)
    {
        ItemToSet = itemToSet;
        SetPosition(itemToSet);
        MenuManager.instance.OpenMenu(MenuType.ThemeChangerMenu);
        Inizialized = true;
    }

    public void ChangeTheme(Palette.VoltageColour theme)
    {
        ItemToSet.SetTheme(theme);
        MenuManager.instance.CloseMenu();
        Inizialized = false;
        ItemToSet = null;
    }

    public void SetPosition(IThemeSettable itemToSet)
    {
        switch (itemToSet)
        {
            case WireDisplay w:
                SetPosition(w);
                break;
            case  SignalDisplay s:
                SetPosition(s);
                break;
        }
    }


    public void SetPosition(WireDisplay wireDisplay)
    {
        ThemeChangerMenuUI.position = InputHelper.MouseWorldPos.Offset(0.2f,-1.8f);
    }


    public void SetPosition(SignalDisplay signalDisplay)
    {
        var e = InputHelper.MouseWorldPos;
        ThemeChangerMenuUI.position = e.x<0 ? e.Offset(1.2f,-1.8f) : e.Offset(-1.2f,-1.8f);
    }
}