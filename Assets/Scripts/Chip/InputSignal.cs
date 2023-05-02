using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using SebInput.Internal;
using UnityEngine;
using UnityEngine.Serialization;

// Provides input signal (0 or 1) to a chip.
// When designing a chip, this input signal can be manually set to 0 or 1 by the player.
[RequireComponent(typeof(SignalDisplay))]
public class InputSignal : ChipSignal
{
    private void ToggleActive()
    {
        if (wireType != Pin.WireType.Simple) return;

        if (State[0] == PinState.HIGH)
            State[0] = PinState.LOW;
        else
            State[0] = PinState.HIGH;
        NotifyStateChange();
    }

    public void SetState(PinStates pinState)
    {
        State = pinState;
        NotifyStateChange();
    }

    public void SendSignal(PinStates signal)
    {
        State = signal;
        outputPins[0].ReceiveSignal(signal);
        NotifyStateChange();
    }

    public void SendSignal()
    {
        outputPins[0].ReceiveSignal(State);
    }


    public override void UpdateSignalName(string newName)
    {
        base.UpdateSignalName(newName);
        outputPins[0].pinName = newName;
    }

    protected override void Start()
    {
        base.Start();
        GetComponentInChildren<SignalEvent>().MouseInteraction.LeftMouseDown += ToggleActive;
    }

    public void SetBusStatus(uint state)
    {
        State = new PinStates(state);
        NotifyStateChange();
    }
}