using System;
using System.Collections.Generic;
using UnityEngine;

public class WireInformation {

	public Wire wire;

	public int startChipIndex;
	public int startChipPinIndex;

	public int endChipIndex;
	public int endChipPinIndex;

	public Vector2[] anchorPoints;

	public WireInformation() {

	}
}

public class CopyPaste : MonoBehaviour {
	List<KeyValuePair<Chip, Vector3>> clipboard = new List<KeyValuePair<Chip, Vector3>>();
	List<WireInformation> wires = new List<WireInformation>();

    void Update() {
		if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.C)) {
			Copy();
		}

		if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.V)) {
			Paste();
		}
	}


	public void Copy() {
		if (Manager.ActiveChipEditor.pinAndWireInteraction.CurrentState != PinAndWireInteraction.State.PasteWires) {
			clipboard.Clear();
			List<Vector3> positions = new List<Vector3>();
			List<Chip> selected = Manager.ActiveChipEditor.chipInteraction.SelectedChips;
			
			wires.Clear();

			foreach (Wire wire in Manager.ActiveChipEditor.pinAndWireInteraction.allWires) {
				WireInformation info = RequiredWire(wire, selected);
				if (info != null) {
					wires.Add(info);
				}
			}

			foreach (Chip chip in selected) {
				positions.Add(chip.transform.position);
			}
			Vector3 center = MathUtility.Center(positions);
			foreach (Chip chip in selected) {
				clipboard.Add(new KeyValuePair<Chip, Vector3>(Manager.instance.GetChipPrefab(chip), chip.transform.position - center));
			}
		}
	}


	public void Paste() {
		if (Manager.ActiveChipEditor.pinAndWireInteraction.CurrentState != PinAndWireInteraction.State.PasteWires) {
			foreach (KeyValuePair<Chip, Vector3> clipboardItem in clipboard) {
				if (clipboardItem.Key is CustomChip custom)
					custom.ApplyWireModes();
			}
				List<Chip> newChips = Manager.ActiveChipEditor.chipInteraction.PasteChips(clipboard);
				Manager.ActiveChipEditor.pinAndWireInteraction.PasteWires(wires, newChips);
		}
		
	}


	public WireInformation RequiredWire(Wire wire, List<Chip> chips) {
		List<Pin> inputs = new List<Pin>();
		List<Pin> outputs = new List<Pin>();

		foreach (Chip chip in chips) {
			inputs.AddRange(chip.inputPins);
			outputs.AddRange(chip.outputPins);
		}

		if (inputs.Contains(wire.endPin) && outputs.Contains(wire.startPin)) {
			WireInformation info = new WireInformation();

			info.wire = wire;

			List<Vector2> anchorPoints = new List<Vector2>();
			for (int i = 0; i < wire.lineRenderer.positionCount; i++) {
				anchorPoints.Add(wire.lineRenderer.GetPosition(i));
			}
			info.anchorPoints = anchorPoints.ToArray();

			info.endChipIndex = chips.IndexOf(wire.endPin.chip);
			info.startChipIndex = chips.IndexOf(wire.startPin.chip);

			info.endChipPinIndex = Array.IndexOf(chips[info.endChipIndex].inputPins, wire.endPin);
			info.startChipPinIndex = Array.IndexOf(chips[info.startChipIndex].outputPins, wire.startPin);
			
			return info;
		}
		return null;
	}

}
