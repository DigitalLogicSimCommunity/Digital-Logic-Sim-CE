using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ChipPackageDisplay : MonoBehaviour
{
    public TMPro.TextMeshPro nameText;
    public Transform container;
    private SpawnableChip SpawnableChip;


    void Awake()
    {
        ScalingManager.i.OnScaleChange += SetSizeAndSpacing;
        Init();
    }

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= SetSizeAndSpacing;
    }

    public void Init()
    {
        SpawnableChip = GetComponent<SpawnableChip>();
        SetUpDisplay();
    }

    private void SetUpDisplay()
    {
        if (SpawnableChip == null) return;

        SetSizeAndSpacing();
        SetDisplay(SpawnableChip.PackageGraphicData);
    }

    private void Start()
    {
        nameText.fontSize = ScalingManager.PackageFontSize;
    }

    public void SetUpForCustomPackageChip(ChipData data)
    {
        gameObject.name = data.name;
        nameText.text = data.name;
        nameText.color = data.NameColour;
        SetColour(data.Colour);
    }

    private void SetColour(Color dataColour)
    {
        container.GetComponent<MeshRenderer>().material.color = dataColour;
    }


    private void SetSizeAndSpacing()
    {
        if (SpawnableChip == null) return;

        SpawnableChip chip = SpawnableChip;
        var data = chip.PackageGraphicData;

        float PinRadius = PinDisplay.radius * 0.25f;
        float PinInteraction = PinRadius * PinDisplay.IteractionFactor;

        nameText.fontSize = ScalingManager.PackageFontSize;


        float containerHeightPadding = 0;
        float containerWidthPadding = 0.1f;
        float pinSpacePadding = PinRadius * 0.2f;
        float containerWidth = nameText.preferredWidth + PinInteraction * 2f + containerWidthPadding;

        int numPins = Mathf.Max(chip.inputPins.Count, chip.outputPins.Count);
        float unpaddedContainerHeight = numPins * (PinRadius * 2 + pinSpacePadding);
        float containerHeight = Mathf.Max(unpaddedContainerHeight, nameText.preferredHeight + 0.05f) +
                                containerHeightPadding;
        float topPinY = unpaddedContainerHeight / 2 - PinRadius;
        float bottomPinY = -unpaddedContainerHeight / 2 + PinRadius;
        const float z = -0.05f;

        // Input pins
        int numInputPinsToAutoPlace = chip.inputPins.Count;
        for (int i = 0; i < numInputPinsToAutoPlace; i++)
        {
            float percent = 0.5f;
            if (chip.inputPins.Count > 1)
            {
                percent = i / (numInputPinsToAutoPlace - 1f);
            }

            if (data.OverrideWidthAndHeight)
            {
                float posX = -data.Width / 2f;
                float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                chip.inputPins[i].transform.localPosition = new Vector3(posX, posY, z);
            }
            else
            {
                float posX = -containerWidth / 2f;
                float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                chip.inputPins[i].transform.localPosition = new Vector3(posX, posY, z);
            }
        }

        // Output pins
        for (int i = 0; i < chip.outputPins.Count; i++)
        {
            float percent = 0.5f;
            if (chip.outputPins.Count > 1)
            {
                percent = i / (chip.outputPins.Count - 1f);
            }

            float posX = containerWidth / 2f;
            float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
            chip.outputPins[i].transform.localPosition = new Vector3(posX, posY, z);
        }

        // Set container size
        if (data.OverrideWidthAndHeight)
        {
            container.transform.localScale =
                new Vector3(data.Width, data.Height, 1);
            GetComponent<BoxCollider2D>().size =
                new Vector2(data.Width, data.Height);
        }
        else
        {
            container.transform.localScale = new Vector3(containerWidth, containerHeight, 1);
            GetComponent<BoxCollider2D>().size = new Vector2(containerWidth, containerHeight);
        }
    }

    private void OnValidate()
    {
        SetSizeAndSpacing();
    }


    void SetDisplay(PackageGraphicData data)
    {
        SetColour(data.PackageColour);
    }
}