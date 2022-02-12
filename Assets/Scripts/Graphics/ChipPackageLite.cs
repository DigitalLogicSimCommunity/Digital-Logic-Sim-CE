using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipPackageLite : MonoBehaviour {

  public enum ChipType { User, Basic, Advanced }
  ;

  public ChipType chipType;
  public Transform container;
  public Pin chipPinPrefab;

  protected const string pinHolderName = "Pin Holder";

  protected virtual void Awake() {
    if (chipType != ChipType.User) {
      BuiltinChip builtinChip = GetComponent<BuiltinChip>();
    }
  }

  public virtual void PackageCustomChip(ChipEditor chipEditor) {
    gameObject.name = chipEditor.chipName;

    // Add and set up the custom chip component
    CustomChip chip = gameObject.AddComponent<CustomChip>();
    chip.chipName = chipEditor.chipName;
    chip.folderName = chipEditor.chipFolder;

    // Set input signals
    chip.inputSignals = new InputSignal[chipEditor.inputsEditor.signals.Count];
    for (int i = 0; i < chip.inputSignals.Length; i++) {
      chip.inputSignals[i] = (InputSignal)chipEditor.inputsEditor.signals[i];
    }

    // Set output signals
    chip.outputSignals =
        new OutputSignal[chipEditor.outputsEditor.signals.Count];
    for (int i = 0; i < chip.outputSignals.Length; i++) {
      chip.outputSignals[i] = (OutputSignal)chipEditor.outputsEditor.signals[i];
    }

    // Create pins and set set package size
    SpawnPins(chip);

    // Parent chip holder to the template, and hide
    Transform implementationHolder = chipEditor.chipImplementationHolder;

    implementationHolder.parent = transform;
    implementationHolder.localPosition = Vector3.zero;
    implementationHolder.gameObject.SetActive(false);
  }

  public void SpawnPins(CustomChip chip) {
    Transform pinHolder = new GameObject(pinHolderName).transform;
    pinHolder.parent = transform;
    pinHolder.localPosition = Vector3.zero;

    chip.inputPins = new Pin[chip.inputSignals.Length];
    chip.outputPins = new Pin[chip.outputSignals.Length];

    for (int i = 0; i < chip.inputPins.Length; i++) {
      Pin inputPin = Instantiate(chipPinPrefab, pinHolder.position,
                                 Quaternion.identity, pinHolder);
      inputPin.pinType = Pin.PinType.ChipInput;
      inputPin.chip = chip;
      inputPin.pinName = chip.inputSignals[i].outputPins[0].pinName;
      chip.inputPins[i] = inputPin;
    }

    for (int i = 0; i < chip.outputPins.Length; i++) {
      Pin outputPin = Instantiate(chipPinPrefab, pinHolder.position,
                                  Quaternion.identity, pinHolder);
      outputPin.pinType = Pin.PinType.ChipOutput;
      outputPin.chip = chip;
      outputPin.pinName = chip.outputSignals[i].inputPins[0].pinName;
      chip.outputPins[i] = outputPin;
    }
  }
}
