using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedOutputPin {
	public string name;
	public Pin.WireType wireType;

	public SavedOutputPin (ChipInstanceHolder chipInstanceHolder, Pin pin) {
		name = pin.pinName;
		wireType = pin.wireType;
	}
}