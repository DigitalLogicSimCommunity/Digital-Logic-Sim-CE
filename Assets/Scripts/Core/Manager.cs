using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChipEditorMode { Create, Update }
;
public class Manager : MonoBehaviour {
    public static ChipEditorMode chipEditorMode;

    public event System.Action<Chip> customChipCreated;
    public event System.Action<Chip> customChipUpdated;

    public ChipEditor chipEditorPrefab;
    public ChipPackage chipPackagePrefab;
    public Wire wirePrefab;
    public Chip[] builtinChips;
    public List<Chip> spawnableChips;
    public UIManager UIManager;

    ChipEditor activeChipEditor;
    int currentChipCreationIndex;
    public static Manager instance;

    void Awake() {
        instance = this;
    }

    void Start() {
        spawnableChips = new List<Chip>();
        activeChipEditor = FindObjectOfType<ChipEditor>();
        SaveSystem.Init();
        SaveSystem.LoadAll(this);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            Pin[] unconnectedInputs =
                activeChipEditor.chipInteraction.UnconnectedInputPins;
            Pin[] unconnectedOutputs =
                activeChipEditor.chipInteraction.UnconnectedOutputPins;
            if (unconnectedInputs.Length > 0) {
                Debug.Log("Found " + unconnectedInputs.Length.ToString() +
                          " unconnected input pins!");
            }
            if (unconnectedOutputs.Length > 0) {
                Debug.Log("Found " + unconnectedOutputs.Length.ToString() +
                          " unconnected output pins!");
            }
        }
    }

    public static ChipEditor ActiveChipEditor {
        get {
            return instance.activeChipEditor;
        }
    }

    public Chip GetChipPrefab(Chip chip) {
        foreach (Chip prefab in builtinChips) {
            if (chip.chipName == prefab.chipName) {
                return prefab;
            }
        }
        foreach (Chip prefab in spawnableChips) {
            if (chip.chipName == prefab.chipName) {
                return prefab;
            }
        }
        return null;
    }

    public static Chip GetCustomChipByName(string name) {
        foreach (Chip chip in instance.spawnableChips) {
            if (name == chip.chipName) {
                return chip;
            }
        }
        return null;
    }

    public Chip LoadChip(ChipSaveData loadedChipData) {
        activeChipEditor.LoadFromSaveData(loadedChipData);
        currentChipCreationIndex = activeChipEditor.creationIndex;

        Chip loadedChip = PackageChip();
        if (loadedChip is CustomChip custom) {
            custom.ApplyWireModes();
        }

        LoadNewEditor();
        return loadedChip;
    }

    public void ViewChip(Chip chip) {
        ChipSaveData chipSaveData = ChipLoader.GetChipSaveData(
                                        chip, builtinChips, spawnableChips, wirePrefab, activeChipEditor);
        LoadNewEditor();
        chipEditorMode = ChipEditorMode.Update;
        UIManager.SetEditorMode(chipEditorMode);
        activeChipEditor.LoadFromSaveData(chipSaveData);
    }

    public void SaveAndPackageChip() {
        ChipSaver.Save(activeChipEditor);
        PackageChip();
        LoadNewEditor();
    }

    public void UpdateChip() {
        Chip updatedChip = TryPackageAndReplaceChip(activeChipEditor.chipName);
        ChipSaver.Update(activeChipEditor, updatedChip);
        LoadNewEditor();
    }

    void SetupPseudoInput(Chip customChip) {
        // TODO: Implement this
        //  if (customChip is CustomChip custom) {
        //  	custom.unconnectedInputs =
        //  activeChipEditor.chipInteraction.UnconnectedInputPins; 	Pin pseudoPin =
        //  Instantiate(chipPackagePrefab.chipPinPrefab.gameObject, parent:
        //  customChip.transform).GetComponent<Pin>(); 	pseudoPin.pinName =
        //  "PseudoInput"; 	pseudoPin.wireType = Pin.WireType.Simple;
        //  	custom.pseudoInput = pseudoPin;
        //  	pseudoPin.chip = customChip;
        //  	foreach (Pin pin in custom.unconnectedInputs) {
        //  		Pin.MakeConnection(pseudoPin, pin);
        //  	}
        //  }
    }

    Chip PackageChip() {
        ChipPackage package = Instantiate(chipPackagePrefab, parent: transform);
        package.PackageCustomChip(activeChipEditor);
        package.gameObject.SetActive(false);

        Chip customChip = package.GetComponent<Chip>();
        SetupPseudoInput(customChip);

        customChipCreated?.Invoke(customChip);
        currentChipCreationIndex++;
        spawnableChips.Add(customChip);
        return customChip;
    }

    Chip TryPackageAndReplaceChip(string original) {
        ChipPackage oldPackage = Array.Find(
                                     GetComponentsInChildren<ChipPackage>(true), cp => cp.name == original);
        if (oldPackage != null) {
            Destroy(oldPackage.gameObject);
        }

        ChipPackage package = Instantiate(chipPackagePrefab, parent: transform);
        package.PackageCustomChip(activeChipEditor);
        package.gameObject.SetActive(false);

        Chip customChip = package.GetComponent<Chip>();

        SetupPseudoInput(customChip);

        int index = spawnableChips.FindIndex(c => c.chipName == original);
        if (index >= 0) {
            spawnableChips[index] = customChip;
            customChipUpdated?.Invoke(customChip);
        }

        return customChip;
    }

    public void ResetEditor() {
        chipEditorMode = ChipEditorMode.Create;
        UIManager.SetEditorMode(chipEditorMode);
        LoadNewEditor();
    }

    void LoadNewEditor() {
        if (activeChipEditor) {
            Destroy(activeChipEditor.gameObject);
            UIManager.SetEditorMode(chipEditorMode);
        }
        activeChipEditor =
            Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);

        activeChipEditor.inputsEditor.CurrentEditor = activeChipEditor;
        activeChipEditor.outputsEditor.CurrentEditor = activeChipEditor;

        activeChipEditor.creationIndex = currentChipCreationIndex;

        Simulation.instance.ResetSimulation();
        ScalingManager.scale = 1;
        FindObjectOfType<ChipEditorOptions>().SetUIValues(activeChipEditor);
    }

    public void SpawnChip(Chip chip) {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        activeChipEditor.chipInteraction.SpawnChip(chip);
    }

    public void LoadMainMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public List<string> AllChipNames(bool builtin = true, bool custom = true) {
        List<string> allChipNames = new List<string>();
        if (builtin) {
            foreach (Chip chip in builtinChips) {
                allChipNames.Add(chip.chipName);
            }
        }
        if (custom) {
            foreach (Chip chip in spawnableChips) {
                allChipNames.Add(chip.chipName);
            }
        }
        return allChipNames;
    }
}
