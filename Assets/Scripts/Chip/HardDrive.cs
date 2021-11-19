using System.Collections.Generic;
using UnityEngine;

public class HardDrive : BuiltinChip {
	public static Dictionary<string, List<int>> contents;

	protected override void Awake() {
		base.Awake();
		contents = SaveSystem.LoadHDDContents();
	}

	protected override void ProcessOutput() {
		switch(inputPins[0].State) {
			case 0:
				string binary = "";
				for (int i = 1; i < 5; i++) {
					binary += inputPins[i].State.ToString();
				}
				if(contents.ContainsKey(binary)) {
					for (int i = 0; i < outputPins.Length; i++)
					{
						outputPins[i].ReceiveSignal(contents[binary][i]);
					}
				} else {
					for (int i = 0; i < outputPins.Length; i++) {
						outputPins[0].ReceiveSignal(0);
					}
				}
				break;
			case 1:
				bool updateFile = false;
				string address = "";
				List<int> store = new List<int>();
				for (int i = 5; i < 13; i++) {
					store.Add(inputPins[i].State);
				}
				for (int i = 1; i < 5; i++) {
					address += inputPins[i].State;
				}
				if(contents.ContainsKey(address)) {
					if (contents[address] != store) {
						updateFile = true;
					}
					contents.Remove(address);
				} else {
					updateFile = true;
				} 
				contents.Add(address, store);
				if (updateFile) {
					SaveSystem.SaveHDDContents(contents);
				}
				break;
			default:
				foreach(Pin i in outputPins) {
					i.ReceiveSignal(0);
				}
				break;
		}
	}
}