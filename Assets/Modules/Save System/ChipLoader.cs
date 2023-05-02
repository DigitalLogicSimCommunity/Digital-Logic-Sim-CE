using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public static class ChipLoader
{
    public static SavedChip[] GetAllSavedChips(string[] chipPaths)
    {
        var savedChips = new SavedChip[chipPaths.Length];

        // Read saved chips from file
        for (var i = 0; i < chipPaths.Length; i++)
        {
            var chipSaveString = SaveSystem.ReadFile(chipPaths[i]);
            SaveCompatibility.FixSaveCompatibility(ref chipSaveString);
            savedChips[i] = JsonUtility.FromJson<SavedChip>(chipSaveString);
        }

        foreach (var chip in savedChips)
            chip.ValidateDefaultData();

        return savedChips;
    }

    public static Dictionary<string, SavedChip> GetAllSavedChipsDic(string[] chipPaths)
    {
        return GetAllSavedChips(chipPaths).ToDictionary(chip => chip.Data.name);
    }

    public static async void LoadAllChips(string[] chipPaths, Manager manager)
    {
        var builtinChips = manager.SpawnableBuiltinChips;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var chipsToLoadDic = GetAllSavedChipsDic(chipPaths);

        var progressBar = ProgressBar.New("Loading All Chips...", wholeNumbers: true);
        progressBar.Open(0, chipsToLoadDic.Count + builtinChips.Length);
        progressBar.SetValue(0, "Start Loading...");

        // Maintain dictionary of loaded chips (initially just the built-in chips)
        var loadedChips = new Dictionary<string, Chip>();
        var i = 0;
        for (; i < builtinChips.Length; i++)
        {
            var builtinChip = builtinChips[i];
            progressBar.SetValue(i, $"Loading '{builtinChip.chipName}'...");
            loadedChips.Add(builtinChip.chipName, builtinChip);
            await Task.Yield();
        }

        foreach (var chip in chipsToLoadDic)
        {
            progressBar.SetValue(i, $"Loading '{chip.Value.Data.name}'...");
            if (!loadedChips.ContainsKey(chip.Key))
            {
                try
                {
                    ResolveDependency(chip.Value);
                }
                catch (Exception e)
                {
                    DLSLogger.LogWarning($"Custom Chip '{chip.Value.Data.name}' could not be loaded!", e.ToString());
                }
            }

            await Task.Yield();
        }

        progressBar.SetValue(progressBar.progressBar.maxValue, "Done!");
        progressBar.Close();
        DLSLogger.Log($"Load time: {sw.ElapsedMilliseconds}ms");

        // the simulation will never create Cyclic path so simple ricorsive descending graph explore shuld be fine
        async void ResolveDependency(SavedChip chip)
        {
            foreach (var dependency in chip.ChipDependecies)
            {
                if (string.Equals(dependency, "SIGNAL IN") || string.Equals(dependency, "SIGNAL OUT")) continue;
                if (loadedChips.ContainsKey(dependency)) continue;

                ResolveDependency(chipsToLoadDic[dependency]);
                await Task.Yield();
                i++;
            }


            if (loadedChips.ContainsKey(chip.Data.name)) return;
            Chip loadedChip = manager.LoadCustomChip(LoadChip(chip, loadedChips));
            loadedChips.Add(loadedChip.chipName, loadedChip);
        }
    }

    // Instantiates all components that make up the given chip, and connects them
    // up with wires The components are parented under a single "holder" object,
    // which is returned from the function
    static ChipInstanceHolder LoadChip(SavedChip chipToLoad, Dictionary<string, Chip> loadedChips)
    {
        bool WouldLoad(out List<string> ComponentsMissing)
        {
            ComponentsMissing = new List<string>();
            foreach (var dependency in chipToLoad.ChipDependecies)
            {
                if (string.Equals(dependency, "SIGNAL IN") || string.Equals(dependency, "SIGNAL OUT")) continue;
                if (!loadedChips.ContainsKey(dependency))
                    ComponentsMissing.Add(dependency);
            }

            return ComponentsMissing.Count <= 0;
        }


        if (WouldLoad(out List<string> miss)) return LoadChipWithWires(chipToLoad, loadedChips);

        string MissingComp = "";
        for (int i = 0; i < miss.Count; i++)
        {
            MissingComp += miss[i];
            if (i < miss.Count - 1)
                MissingComp += ",";
        }

        DLSLogger.LogError($"Failed to load {chipToLoad.Data.name} sub component: {MissingComp} was missing");

        return null;
    }

    static ChipInstanceHolder LoadChipWithWires(SavedChip chipToLoad, Dictionary<string, Chip> loadedChips,
        ChipEditor chipEditor = null)
    {
        if (chipEditor == null)
            chipEditor = Manager.ActiveChipEditor;

        ChipInstanceHolder loadedChipData = new ChipInstanceHolder();
        int numComponents = chipToLoad.savedComponentChips.Length;
        loadedChipData.componentChips = new Chip[numComponents];
        loadedChipData.Data = chipToLoad.Data;
        List<Wire> wiresToLoad = new List<Wire>();

        // Spawn component chips (the chips used to create this chip)
        // These will have been loaded already, and stored in the
        // loadedChips dictionary
        for (int i = 0; i < numComponents; i++)
        {
            SavedComponentChip savedComponentToLoad = chipToLoad.savedComponentChips[i];
            string componentName = savedComponentToLoad.chipName;
            Vector2 pos = new Vector2((float)savedComponentToLoad.posX, (float)savedComponentToLoad.posY);

            if (!loadedChips.ContainsKey(componentName))
                DLSLogger.LogError(
                    $"Failed to load sub component: {componentName} While loading {chipToLoad.Data.name}");

            
            Chip instanceComponent = chipEditor.LoadInstanceData(loadedChips[componentName],pos,Quaternion.identity);
            instanceComponent.gameObject.SetActive(true);

            // Load input pin names
            for (int inputIndex = 0;
                 inputIndex < savedComponentToLoad.inputPins.Length &&
                 inputIndex < instanceComponent.inputPins.Count;
                 inputIndex++)
            {
                instanceComponent.inputPins[inputIndex].pinName =
                    savedComponentToLoad.inputPins[inputIndex].name;
                instanceComponent.inputPins[inputIndex].wireType =
                    savedComponentToLoad.inputPins[inputIndex].wireType;
            }

            // Load output pin names
            for (int ouputIndex = 0;
                 ouputIndex < savedComponentToLoad.outputPins.Length &&
                 ouputIndex < instanceComponent.outputPins.Count;
                 ouputIndex++)
            {
                instanceComponent.outputPins[ouputIndex].pinName =
                    savedComponentToLoad.outputPins[ouputIndex].name;
                instanceComponent.outputPins[ouputIndex].wireType =
                    savedComponentToLoad.outputPins[ouputIndex].wireType;
            }

            loadedChipData.componentChips[i] = instanceComponent;
        }

        // Connect pins with wires
        for (int chipIndex = 0;
             chipIndex < chipToLoad.savedComponentChips.Length;
             chipIndex++)
        {
            Chip loadedComponentChip = loadedChipData.componentChips[chipIndex];
            for (int inputPinIndex = 0;
                 inputPinIndex < loadedComponentChip.inputPins.Count &&
                 inputPinIndex < chipToLoad.savedComponentChips[chipIndex].inputPins.Length;
                 inputPinIndex++)
            {
                SavedInputPin savedPin =
                    chipToLoad.savedComponentChips[chipIndex].inputPins[inputPinIndex];
                Pin pin = loadedComponentChip.inputPins[inputPinIndex];

                // If this pin should receive input from somewhere, then wire it up to
                // that pin
                if (savedPin.parentChipIndex == -1) continue;
                Pin connectedPin =
                    loadedChipData.componentChips[savedPin.parentChipIndex]
                        .outputPins[savedPin.parentChipOutputIndex];
                pin.cyclic = savedPin.isCylic;

                if (!Pin.TryConnect(connectedPin, pin)) continue;
                Wire loadedWire = chipEditor.LoadWire(connectedPin, pin);
                wiresToLoad.Add(loadedWire);
            }
        }

        loadedChipData.wires = wiresToLoad.ToArray();

        return loadedChipData;
    }

    public static ChipInstanceHolder GetChipInstanceData(Chip chip, ChipEditor chipEditor)
    {
        // @NOTE: chipEditor can be removed here if:
        //     * Chip & wire instatiation is inside their respective implementation
        //     holders is inside the chipEditor
        //     * the wire connections are done inside ChipEditor.LoadFromSaveData
        //     instead of ChipLoader.LoadChipWithWires

        SavedChip chipToTryLoad = SaveSystem.ReadChip(chip.chipName);

        if (chipToTryLoad == null)
            return null;

        ChipInstanceHolder loadedChipData =LoadChipWithWires(chipToTryLoad, Manager.instance.AllSpawnableChipDic(), chipEditor);
        SavedWireLayout wireLayout = SaveSystem.ReadWire(loadedChipData.Data.name);

        //Work Around solution. it just Work but maybe is worth to change the entire way to save WireLayout (idk i don't think so)
        for (int i = 0; i < loadedChipData.wires.Length; i++)
        {
            Wire wire = loadedChipData.wires[i];
            wire.endPin.pinName += i;
        }

        // Set wires anchor points
        foreach (SavedWire wire in wireLayout.serializableWires)
        {
            string startPinName;
            string endPinName;

            // This fixes a bug which caused chips to be unable to be viewed/edited if
            // some of input/output pins were swaped.
            try
            {
                startPinName = loadedChipData.componentChips[wire.parentChipIndex]
                    .outputPins[wire.parentChipOutputIndex]
                    .pinName;
                endPinName = loadedChipData.componentChips[wire.childChipIndex]
                    .inputPins[wire.childChipInputIndex]
                    .pinName;
            }
            catch (IndexOutOfRangeException)
            {
                // Swap input pins with output pins.
                startPinName = loadedChipData.componentChips[wire.parentChipIndex]
                    .inputPins[wire.parentChipOutputIndex]
                    .pinName;
                endPinName = loadedChipData.componentChips[wire.childChipIndex]
                    .outputPins[wire.childChipInputIndex]
                    .pinName;
            }

            int wireIndex = Array.FindIndex(loadedChipData.wires,
                w => w.startPin.pinName == startPinName && w.endPin.pinName == endPinName);
            if (wireIndex >= 0)
                loadedChipData.wires[wireIndex].SetAnchorPoints(wire.anchorPoints);
        }

        foreach (var wire in loadedChipData.wires)
        {
            wire.endPin.pinName = wire.endPin.pinName.Remove(wire.endPin.pinName.Length - 1);
        }

        return loadedChipData;
    }

    public static void Import(string path)
    {
        var allChips = SaveSystem.GetAllSavedChips();
        var nameUpdateLookupTable = new Dictionary<string, string>();

        using var reader = new StreamReader(path);
        var numberOfChips = Int32.Parse(reader.ReadLine());

        for (var i = 0; i < numberOfChips; i++)
        {
            string chipName = reader.ReadLine();
            int saveDataLength = Int32.Parse(reader.ReadLine());
            int wireSaveDataLength = Int32.Parse(reader.ReadLine());

            string saveData = "";
            string wireSaveData = "";

            for (int j = 0; j < saveDataLength; j++)
            {
                saveData += reader.ReadLine() + "\n";
            }

            for (int j = 0; j < wireSaveDataLength; j++)
            {
                wireSaveData += reader.ReadLine() + "\n";
            }

            // Rename chip if already exist
            if (Array.FindIndex(allChips, c => c.Data.name == chipName) >= 0)
            {
                int nameCounter = 2;
                string newName;
                do
                {
                    newName = chipName + nameCounter.ToString();
                    nameCounter++;
                } while (Array.FindIndex(allChips, c => c.Data.name == newName) >= 0);

                nameUpdateLookupTable.Add(chipName, newName);
                chipName = newName;
            }

            // Update name inside file if there was some names changed
            foreach (KeyValuePair<string, string> nameToReplace in nameUpdateLookupTable)
            {
                saveData = saveData
                    .Replace("\"name\": \"" + nameToReplace.Key + "\"",
                        "\"name\": \"" + nameToReplace.Value + "\"")
                    .Replace("\"chipName\": \"" + nameToReplace.Key + "\"",
                        "\"chipName\": \"" + nameToReplace.Value + "\"");
            }

            string chipSaveFile = SaveSystem.GetPathToSaveFile(chipName);

            SaveSystem.WriteChip(chipName, saveData);
            SaveSystem.WriteWire(chipName, wireSaveData);
        }
    }
}