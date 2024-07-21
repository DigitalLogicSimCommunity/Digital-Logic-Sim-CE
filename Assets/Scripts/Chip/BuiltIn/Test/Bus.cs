using System.Collections;
using System.Collections.Generic;
using DLS.Core.Simulation;
using UnityEngine;
using UnityEngine.Serialization;

public class Bus : Chip
{
    public MeshRenderer meshRenderer;
    public Palette signalPalette;

    public override void ProcessOutput()
    {
        var outputSignal = PinState.FLOATING;
        foreach (var t in inputPins)
        {
            if (!t.HasParent) continue;
            if (t.State[0] == PinState.FLOATING) continue;
            outputSignal = t.State[0];
        }

        foreach (var t in outputPins)
        {
            t.ReceiveSignal(PinStates.Getstates(outputSignal));
        }

        SetCol(outputSignal);
    }

    void SetCol(PinState pinState)
    {
        var defTheme = signalPalette.GetDefaultTheme();
        meshRenderer.material.color = defTheme.GetColour(PinStates.Getstates(pinState));
    }


    public Pin GetBusConnectionPin(Pin wireStartPin, Vector2 connectionPos)
    {
        Pin connectionPin = null;
        // Wire wants to put data onto bus
        if (wireStartPin != null && wireStartPin.pinType == Pin.PinType.ChipOutput)
        {
            connectionPin = FindUnusedInputPin();
        }
        else
        {
            // Wire wants to get data from bus
            connectionPin = FindUnusedOutputPin();
        }

        var lineCentre = (Vector2)transform.position;
        var pos = MathUtility.ClosestPointOnLineSegment(lineCentre + Vector2.left * 100,
            lineCentre + Vector2.right * 100,
            connectionPos);
        connectionPin.transform.position = pos;
        return connectionPin;
    }

    Pin FindUnusedOutputPin()
    {
        foreach (var t in outputPins)
        {
            if (t.childPins.Count == 0)
            {
                return t;
            }
        }

        Debug.Log("Ran out of pins");
        return null;
    }

    Pin FindUnusedInputPin()
    {
        foreach (var t in inputPins)
        {
            if (t.parentPin == null)
            {
                return t;
            }
        }

        Debug.Log("Ran out of pins");
        return null;
    }
}