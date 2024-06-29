using System.Collections.Generic;
using Core;
using DLS.Core.Simulation;
using UnityEngine;

public class XorGate : BuiltinChip
{
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Gate;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new Color(93, 53, 115, 255)
        };
        inputPins = new List<Pin>(2);
        outputPins = new List<Pin>(1);
        Name = "XOR";
    }

    public override void ProcessOutput()
    {
        PinState outputSignal = inputPins[0].State[0] ^ inputPins[1].State[0];
        outputPins[0].ReceiveSignal(PinStates.Getstates((outputSignal)));
    }

}