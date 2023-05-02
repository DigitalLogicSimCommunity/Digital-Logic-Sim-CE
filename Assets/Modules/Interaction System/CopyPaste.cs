using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WireInformation
{
    public Wire wire;

    public int startChipIndex;
    public int startChipPinIndex;

    public int endChipIndex;
    public int endChipPinIndex;

    public Vector2[] anchorPoints;

    public WireInformation()
    {
    }
}

public class CopyPaste : MonoBehaviour
{
    List<KeyValuePair<Chip, Vector3>> clipboard =
        new List<KeyValuePair<Chip, Vector3>>();

    List<WireInformation> wires = new List<WireInformation>();

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.C))
            Copy();

        if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.V))
            Paste();
    }

    public void Copy()
    {
        if (Manager.PinAndWireInteraction.CurrentState == PinAndWireInteraction.State.PasteWires) return;
        clipboard.Clear();
        List<Chip> selected =
            Manager.ChipInteraction.SelectedChips;

        wires.Clear();

        foreach (Wire wire in Manager.PinAndWireInteraction.allWires)
        {
            WireInformation info = RequiredWire(wire, selected);
            if (info != null)
            {
                wires.Add(info);
            }
        }

        List<Vector3> positions = selected.Select(chip => chip.transform.position).ToList();

        Vector3 center = MathUtility.Center(positions);
        foreach (Chip chip in selected)
        {
            clipboard.Add(new KeyValuePair<Chip, Vector3>(
                Manager.instance.GetChipPrefab(chip),
                chip.transform.position - center));
        }
    }

    public void Paste()
    {
        if (Manager.PinAndWireInteraction.CurrentState == PinAndWireInteraction.State.PasteWires) return;

        foreach (KeyValuePair<Chip, Vector3> clipboardItem in clipboard)
        {
            if (clipboardItem.Key is CustomChip custom)
                custom.ApplyWireModes();
        }

        List<Chip> newChips =
            Manager.ChipInteraction.PasteChips(clipboard);
        Manager.PinAndWireInteraction.PasteWires(wires,
            newChips);
    }

    public WireInformation RequiredWire(Wire wire, List<Chip> chips)
    {
        List<Pin> inputs = new List<Pin>();
        List<Pin> outputs = new List<Pin>();

        foreach (Chip chip in chips)
        {
            inputs.AddRange(chip.inputPins);
            outputs.AddRange(chip.outputPins);
        }

        if (!inputs.Contains(wire.endPin) || !outputs.Contains(wire.startPin)) return null;

        WireInformation info = new WireInformation();

        info.wire = wire;

        info.anchorPoints = Enumerable.ToArray(wire.anchorPoints);

        info.endChipIndex = chips.IndexOf(wire.endPin.chip);
        info.startChipIndex = chips.IndexOf(wire.startPin.chip);

        info.endChipPinIndex = chips[info.endChipIndex].inputPins.FindIndex(x => x == wire.endPin);
        info.startChipPinIndex = chips[info.startChipIndex].outputPins.FindIndex(x => x == wire.startPin);

        return info;
    }
}