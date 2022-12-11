using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for input and output signals
public class ChipSignal : Chip
{

    public bool currentState;

    public Palette palette;
    public MeshRenderer indicatorRenderer;
    public MeshRenderer pinRenderer;
    public MeshRenderer wireRenderer;

    public bool displayGroupDecimalValue { get; set; } = false;
    public bool useTwosComplement { get; set; } = true;
    public Pin.WireType wireType = Pin.WireType.Simple;
    public int GroupID { get; set; } = -1;

    [HideInInspector]
    public string signalName;
    protected bool interactable = true;

    public virtual void SetInteractable(bool interactable)
    {
        this.interactable = interactable;

        if (!interactable)
        {
            indicatorRenderer.material.color = palette.nonInteractableCol;
            pinRenderer.material.color = palette.nonInteractableCol;
            wireRenderer.material.color = palette.nonInteractableCol;
        }
    }

    public void SetDisplayState(bool state)
    {

        if (indicatorRenderer && interactable)
            indicatorRenderer.material.color = state ? palette.onCol : palette.offCol;
    }

    public static bool InSameGroup(ChipSignal signalA, ChipSignal signalB) => (signalA.GroupID == signalB.GroupID) && (signalA.GroupID != -1);



    public virtual void UpdateSignalName(string newName) => signalName = newName;
}