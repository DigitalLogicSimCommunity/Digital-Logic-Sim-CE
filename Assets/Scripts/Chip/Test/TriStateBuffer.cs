using UnityEngine;

public class TriStateBuffer : BuiltinChip {

	protected override void Awake () {
		base.Awake ();
	}

	protected override void ProcessOutput () {
		int data = inputPins[0].State;
		int enable = inputPins[1].State;
		if (enable == 1) {
			outputPins[0].ReceiveSignal (data);
		} else {
			outputPins[0].ReceiveSignal (-1);
		}

	}

}