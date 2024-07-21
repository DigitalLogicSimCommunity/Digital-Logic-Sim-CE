using System.Collections;
using System.Collections.Generic;
using Interaction.Signal.Display;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SavedComponentChip {
	public string chipName;
	public float posX;
	public float posY;

	public SavedInputPin[] inputPins;
	public SavedOutputPin[] outputPins;

	public int signalGroupId =-1;

	public string ThemeName;

	public SavedComponentChip()
	{
	}

	public SavedComponentChip (ChipInstanceHolder chipInstanceHolder, Chip chip) {
		chipName = chip.Name;

        posX = chip.transform.position.x ;
        posY = chip.transform.position.y ;

        if (chip is ChipSignal s)
        {
	        signalGroupId = s.GroupId;
	        ThemeName = s.GetComponentInChildren<SignalDisplay>().CurrentTheme.Name;
        }

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