using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Core.Simulation;
using Interaction.Signal.Display;
using UnityEngine;
using static Pin;

// Base class for input and output signals
[RequireComponent(typeof(SignalDisplay))]
public class ChipSignal : Chip
{
    public int GroupId = -1;
    public event Action<WireType, PinStates> OnStateChange;
    public event Action<bool> OnInteractableSet;

    public WireType wireType = WireType.Simple;
    private PinStates currentState;

    public PinStates State
    {
        get => currentState ??= PinStates.AllLow(wireType);
        protected set => currentState = value;
    }

    protected bool interactable = true;


    [HideInInspector] public string signalName;


    protected override void Start()
    {
        base.Start();
        NotifyStateChange();
    }


    public virtual void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        OnInteractableSet?.Invoke(interactable);
    }

    public virtual void UpdateSignalName(string newName) => signalName = newName;

    protected void NotifyStateChange()
    {
        if (!interactable) return;
        OnStateChange?.Invoke(wireType, State);
    }

    public void ClearStates()
    {
        State = PinStates.AllLow(wireType);
        NotifyStateChange();
    }
}