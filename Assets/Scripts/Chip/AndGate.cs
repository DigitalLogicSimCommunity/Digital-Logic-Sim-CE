using UnityEngine;

public class AndGate : BuiltinChip {

	protected override void Awake () {
		base.Awake ();
	}

	protected override void ProcessOutput () {
		int outputSignal;
		if (inputPins[0].State == 1 && inputPins[1].State ==1)
        {
			outputSignal = 1;
        }
		else
        {
			outputSignal = 0;
        }
		outputPins[0].ReceiveSignal (outputSignal);
	}

}