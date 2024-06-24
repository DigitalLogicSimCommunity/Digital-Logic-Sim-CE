using System.Collections.Generic;
using Core;
using DLS.Core.Simulation;
using UnityEngine;

public class TriStateBuffer : BuiltinChip
{
    
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Miscellaneous;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new Color(0,0,0, 255)
        };
        inputPins = new List<Pin>(2);
        outputPins = new List<Pin>(1);
        Name = "TRI-STATE BUFFER";
    }
    
    protected override void ProcessOutput()
    {
        var data = inputPins[0].State[0];
        var enable = inputPins[1].State[0];
        var pinState = (enable == PinState.HIGH) ? data : PinState.FLOATING;

        outputPins[0].ReceiveSignal(PinStates.Getstates(pinState));
    }
}