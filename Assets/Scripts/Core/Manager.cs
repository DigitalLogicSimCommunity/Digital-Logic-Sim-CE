using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ChipEditorMode { Create, Update }
;
public class Manager : MonoBehaviour
{
    public static ChipEditorMode chipEditorMode;

    public event Action<Chip> customChipCreated;
    public event Action<Chip> customChipUpdated;

    public ChipEditor chipEditorPrefab;
    public ChipPackage chipPackagePrefab;
    public Wire wirePrefab;
    public Chip[] builtinChips;
    public List<Chip> SpawnableCustomChips;
    public UIManager UIManager;

    ChipEditor activeChipEditor;
    int currentChipCreationIndex;
    public static Manager instance;

    void Awake()
    {
        instance = this;
        SaveSystem.Init();
        FolderSystem.Init();
    }

    void Start()
    {
        SpawnableCustomChips = new List<Chip>();
        activeChipEditor = FindObjectOfType<ChipEditor>();
        SaveSystem.LoadAllChips(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Pin[] unconnectedInputs =
                activeChipEditor.chipInteraction.UnconnectedInputPins;
            Pin[] unconnectedOutputs =
                activeChipEditor.chipInteraction.UnconnectedOutputPins;
            if (unconnectedInputs.Length > 0)
            {
                Debug.Log("Found " + unconnectedInputs.Length.ToString() +
                          " unconnected input pins!");
            }
            if (unconnectedOutputs.Length > 0)
            {
                Debug.Log("Found " + unconnectedOutputs.Length.ToString() +
                          " unconnected output pins!");
            }
        }
    }

    public static ChipEditor ActiveChipEditor => instance.activeChipEditor;

    public Chip GetChipPrefab(Chip chip)
    {
        foreach (Chip prefab in builtinChips)
        {
            if (chip.chipName == prefab.chipName)
            {
                return prefab;
            }
        }
        foreach (Chip prefab in SpawnableCustomChips)
        {
            if (chip.chipName == prefab.chipName)
            {
                return prefab;
            }
        }
        return null;
    }

    public static Chip GetChipByName(string name)
    {
        foreach (Chip chip in instance.SpawnableCustomChips)
        {
            if (name == chip.chipName)
            {
                return chip;
            }
        }
        return null;
    }

    public Chip LoadChip(ChipSaveData loadedChipData)
    {
        if (loadedChipData == null) return null;
        activeChipEditor.LoadFromSaveData(loadedChipData);
        currentChipCreationIndex = activeChipEditor.Data.creationIndex;

        Chip loadedChip = PackageChip();
        if (loadedChip is CustomChip custom)
            custom.ApplyWireModes();

        LoadNewEditor();
        return loadedChip;
    }

    public void ViewChip(Chip chip)
    {
        ChipSaveData chipSaveData = ChipLoader.GetChipSaveData(
            chip, builtinChips, SpawnableCustomChips, wirePrefab, activeChipEditor);
        LoadNewEditor();
        chipEditorMode = ChipEditorMode.Update;
        UIManager.SetEditorMode(chipEditorMode);
        activeChipEditor.LoadFromSaveData(chipSaveData);
    }

    public void SaveAndPackageChip()
    {
        ChipSaver.Save(activeChipEditor);
        PackageChip();
        LoadNewEditor();
    }

    public void UpdateChip()
    {
        Chip updatedChip = TryPackageAndReplaceChip(activeChipEditor.Data.name);
        ChipSaver.Update(activeChipEditor, updatedChip);
        chipEditorMode = ChipEditorMode.Create;
        LoadNewEditor();
    }

    void SetupPseudoInput(Chip customChip)
    {
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

    Chip PackageChip()
    {
        ChipPackage package = Instantiate(chipPackagePrefab, parent: transform);
        package.PackageCustomChip(activeChipEditor);
        package.gameObject.SetActive(false);

        var customChip = package.GetComponent<Chip>();
        SetupPseudoInput(customChip);

        if (customChip is CustomChip c)
            c.Init();

        customChipCreated?.Invoke(customChip);
        currentChipCreationIndex++;
        SpawnableCustomChips.Add(customChip);
        return customChip;
    }

    public void ChangeFolderToChip(string ChipName, int index)
    {
        if (SpawnableCustomChips.Where(x => string.Equals(x.name, ChipName)).First() is CustomChip customChip)
            customChip.FolderIndex = index;
    }

    Chip TryPackageAndReplaceChip(string original)
    {
        ChipPackage oldPackage = Array.Find(
            GetComponentsInChildren<ChipPackage>(true), cp => cp.name == original);
        if (oldPackage != null) { Destroy(oldPackage.gameObject); }

        ChipPackage package = Instantiate(chipPackagePrefab, parent: transform);
        package.PackageCustomChip(activeChipEditor);
        package.gameObject.SetActive(false);

        Chip customChip = package.GetComponent<Chip>();

        SetupPseudoInput(customChip);

        int index = SpawnableCustomChips.FindIndex(c => c.chipName == original);
        if (index >= 0)
        {
            SpawnableCustomChips[index] = customChip;
            customChipUpdated?.Invoke(customChip);
        }

        return customChip;
    }

    public void ResetEditor()
    {
        chipEditorMode = ChipEditorMode.Create;
        UIManager.SetEditorMode(chipEditorMode);
        LoadNewEditor();
    }

    void LoadNewEditor()
    {
        if (activeChipEditor)
        {
            Destroy(activeChipEditor.gameObject);
            UIManager.SetEditorMode(chipEditorMode);
        }
        activeChipEditor =
            Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);

        activeChipEditor.inputsEditor.CurrentEditor = activeChipEditor;
        activeChipEditor.outputsEditor.CurrentEditor = activeChipEditor;

        activeChipEditor.Data.creationIndex = currentChipCreationIndex;

        Simulation.instance.ResetSimulation();
        ScalingManager.scale = 1;
        FindObjectOfType<ChipEditorOptions>().SetUIValues(activeChipEditor);
    }

    public void ChipButtonHanderl(Chip chip)
    {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        activeChipEditor.chipInteraction.ChipButtonInteraction(chip);
    }

    public void LoadMainMenu()
    {
        if (chipEditorMode == ChipEditorMode.Update)
        {
            chipEditorMode = ChipEditorMode.Create;
            LoadNewEditor();
        }
        else
        {
            FolderSystem.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public List<string> AllChipNames(bool builtin = true, bool custom = true)
    {
        List<string> allChipNames = new List<string>();
        if (builtin)
        {
            foreach (Chip chip in builtinChips)
            {
                allChipNames.Add(chip.chipName);
            }
        }
        if (custom)
        {
            foreach (Chip chip in SpawnableCustomChips)
            {
                allChipNames.Add(chip.chipName);
            }
        }
        return allChipNames;
    }
}
