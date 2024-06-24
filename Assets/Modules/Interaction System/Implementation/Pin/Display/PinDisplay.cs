using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Core.Simulation;
using UnityEngine;

public class PinDisplay : MonoBehaviour
{
    private new Renderer renderer;

    // Appearance
    private PinInteractionPalette InteractionPalette;

    public static float radius => ScalingManager.PinSize;

    public static float IteractionFactor => 1.1f;
    public static float interactionRadius => radius * IteractionFactor;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }
    

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= UpdateScale;
    }

    private void Start()
    {
        InteractionPalette = ThemeManager.Palette.PinInteractionPalette;
        
        var Pin = GetComponent<PinEvent>();
        // Pin.OnStateChange += UpdateColor;
        Pin.MouseInteraction.MouseEntered += (_) => SelectionAppearance();
        Pin.MouseInteraction.MouseExitted += (_) => NormalAppearance();
        ScalingManager.i.OnScaleChange += UpdateScale;
        
        
        renderer.material.color = InteractionPalette.PinDefaultColor;
        UpdateScale();
    }


    private void UpdateScale()
    {
        transform.localScale = Vector3.one * (radius * 2);
    }

    private void SetColor(Color newColor)
    {
        var material = renderer.material;
        if (material.color != newColor)
        {
            material.color = newColor;
        }
    }

    private void SelectionAppearance()
    {
        transform.localScale = Vector3.one * (interactionRadius * 2);
        SetColor(InteractionPalette.PinHighlighte);
    }

    private void NormalAppearance()
    {
        transform.localScale = Vector3.one * (radius * 2);
        SetColor(InteractionPalette.PinDefaultColor);
    }
}