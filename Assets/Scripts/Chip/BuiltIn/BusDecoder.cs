using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DLS.Simulation;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.Chip
{
	public class BusDecoder : BuiltinChip
	{
		public override void Init()
		{
			base.Init();
			ChipType = ChipType.Miscellaneous;
			PackageGraphicData = new PackageGraphicData()
			{
				PackageColour = new Color(255, 135, 0, 255)
			};
			inputPins = new List<Pin>(1);
			outputPins = new List<Pin>(4);
			chipName = "4 BIT DECODER";
		}
		protected override void ProcessOutput()
		{
			var inputSignal = inputPins[0].State.ToUInt();
			outputPins.Reverse();
			foreach(var outputPin in outputPins)
			{
				outputPin.ReceiveSignal(PinStates.Getstates(((inputSignal & 1) ==1)? PinState.HIGH: PinState.LOW));
				inputSignal = (inputSignal>>1);
			}

			outputPins.Reverse();
		}
	}
}
