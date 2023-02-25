using UnityEngine;

public class OrGate : BuiltinChip {

	protected override void Awake () {
		base.Awake ();
	}

	protected override void ProcessOutput () {
		uint outputSignal = inputPins[0].State | inputPins[1].State;
		outputPins[0].ReceiveSignal (outputSignal);
	}

}