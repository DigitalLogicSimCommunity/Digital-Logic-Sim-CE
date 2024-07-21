using Interaction.Signal.Display;
using UnityEngine;

// Output signal of a chip.
[RequireComponent(typeof(SignalDisplay))]
public class OutputSignal : ChipSignal {



	public override void ReceiveInputSignal (Pin inputPin) {
		State = inputPin.State;
		NotifyStateChange();
	}

	public override void UpdateSignalName (string newName) {
		base.UpdateSignalName (newName);
		inputPins[0].pinName = newName;
	}

}