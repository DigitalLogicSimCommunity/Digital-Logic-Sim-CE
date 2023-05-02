using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedComponentChip {
	public string chipName;
	public float posX;
	public float posY;

	public SavedInputPin[] inputPins;
	public SavedOutputPin[] outputPins;

	public SavedComponentChip (ChipInstanceHolder chipInstanceHolder, Chip chip) {
		chipName = chip.chipName;

        posX = chip.transform.position.x ;
        posY = chip.transform.position.y ;

        // Input pins
        inputPins = new SavedInputPin[chip.inputPins.Count];
		for (int i = 0; i < inputPins.Length; i++)
			inputPins[i] = new SavedInputPin (chipInstanceHolder, chip.inputPins[i]);

		// Output pins
		outputPins = new SavedOutputPin[chip.outputPins.Count];
		for (int i = 0; i < chip.outputPins.Count; i++) 
			outputPins[i] = new SavedOutputPin(chipInstanceHolder, chip.outputPins[i]);
	}

}