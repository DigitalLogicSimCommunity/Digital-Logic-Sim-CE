using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThemeManager : MonoBehaviour
{
    public event Action OnDefaultThemeChange;
    public static ThemeManager instance;
    public static Palette Palette => instance.palette;
    public static Palette.VoltageColour DefaultTheme => instance.palette.GetDefaultTheme();
    public Palette palette;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetOrderForTheme();
    }

    private void SetOrderForTheme()
    {
        var zPriority = 1.5f;
        var offset = 0.1f;
        foreach (var colour in palette.voltageColours)
        {
            colour.DisplayPriority = zPriority + offset;
            zPriority += offset;
        }
    }

    private void OnValidate()
    {
        SetOrderForTheme(); 
    }

    private void Update()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.E))
        {
            palette.DefaultIndex++;
            OnDefaultThemeChange?.Invoke();
        }

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Q))
        {
            OnDefaultThemeChange?.Invoke();
            palette.DefaultIndex--;
        }
    }
}
