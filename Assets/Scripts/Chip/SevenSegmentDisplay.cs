using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SevenSegmentDisplay : BuiltinChip
{
    [SerializeField] MeshRenderer[] segments;
    public Color offCol;
    public Color onCol;
    public Color highlightCol;

    void Update()
    {
        SetSize();
    }

    private void SetSize()
    {
        var package = GetComponent<ChipPackage>();
        // Limit size reduction so we can still read the thing.
        if (package != null && ScalingManager.scale > 0.45f)
        {
            package.override_width_and_height = true;
            package.override_width =  ScalingManager.scale * 1.8f;
            package.override_height = ScalingManager.scale *2.0f;
            package.SetSizeAndSpacing(this);
        }

    }


    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].sharedMaterial = Material.Instantiate(segments[i].sharedMaterial);
            segments[i].sharedMaterial.color = offCol;
        }
        SetSize();
    }

    protected override void ProcessOutput()
    {
        for (int i = 0; i < inputPins.Length  ; i++)
        {
            Color col = inputPins[i].State == 1 ? onCol : offCol;
            segments[i].sharedMaterial.color = col;
            inputPins[i].tellPinSimIsOff();
        }
    }
}

