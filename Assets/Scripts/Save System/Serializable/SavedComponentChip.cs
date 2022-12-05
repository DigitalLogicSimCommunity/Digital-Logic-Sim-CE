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

	public SavedComponentChip (ChipSaveData chipSaveData, Chip chip) {
		chipName = chip.chipName;

        posX = chip.transform.position.x ;
        posY = chip.transform.position.y ;

        // Input pins
        inputPins = new SavedInputPin[chip.inputPins.Length];
		for (int i = 0; i < inputPins.Length; i++)
			inputPins[i] = new SavedInputPin (chipSaveData, chip.inputPins[i]);

		// Output pins
		outputPins = new SavedOutputPin[chip.outputPins.Length];
		for (int i = 0; i < chip.outputPins.Length; i++) 
			outputPins[i] = new SavedOutputPin(chipSaveData, chip.outputPins[i]);
	}

}