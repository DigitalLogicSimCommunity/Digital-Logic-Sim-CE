using System;
using System.Collections.Generic;
using Core;
using DLS.Core.Simulation;
using UnityEngine;

public class NotGate : BuiltinChip
{
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Gate;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new Color(140, 43, 36, 255)
        };
        inputPins = new List<Pin>(1);
        outputPins = new List<Pin>(1);
        chipName = "NOT";
    }

    protected override void ProcessOutput()
    {
        PinState outputSignal;
        switch (inputPins[0].State[0])
        {
            case PinState.HIGH:
                outputSignal = PinState.LOW;
                break;
            case PinState.LOW:
                outputSignal = PinState.HIGH;
                break;
            case PinState.FLOATING:
                outputSignal = PinState.FLOATING;
                break;
            default:
                outputSignal = PinState.FLOATING;
                break;
        }

        outputPins[0].ReceiveSignal(PinStates.Getstates(outputSignal));
    }
}