using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for input and output signals
public class ChipSignal : Chip
{

    public int currentState;

    public Palette palette;
    public MeshRenderer indicatorRenderer;
    public MeshRenderer pinRenderer;
    public MeshRenderer wireRenderer;
    public TMPro.TextMeshProUGUI busReadout;

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

    public void SetDisplayState(int state)
    {
        if (indicatorRenderer && interactable)
        {
            indicatorRenderer.material.color = wireType == Pin.WireType.Simple ? 
                (state == 1 ? palette.onCol : palette.offCol) :
                (state > 0 ? palette.busColor : palette.offCol);
            if (state == 0 || wireType == Pin.WireType.Simple) busReadout.gameObject.SetActive(false);
            else busReadout.gameObject.SetActive(true);
            busReadout.text = state.ToString();
        }
    }

    public static bool InSameGroup(ChipSignal signalA, ChipSignal signalB) => (signalA.GroupID == signalB.GroupID) && (signalA.GroupID != -1);



    public virtual void UpdateSignalName(string newName) => signalName = newName;
}