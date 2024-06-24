using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using DLS.SaveSystem.Serializable.SerializationHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ChipLoader
{


    public static async void LoadAllChips( IDictionary<string, SavedChip> chipsToLoadDic, Manager manager)
    {
        var builtinChips = manager.SpawnableBuiltinChips;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var progressBar = ProgressBar.New("Loading All Chips...", wholeNumbers: true);
        progressBar.Open(0, chipsToLoadDic.Count + builtinChips.Length);
        progressBar.SetValue(0, "Start Loading...");

        // Maintain dictionary of loaded chips (initially just the built-in chips)
        var loadedChips = new Dictionary<string, Chip>();
        var i = 0;
        for (; i < builtinChips.Length; i++)
        {
            var builtinChip = builtinChips[i];
            progressBar.SetValue(i, $"Loading '{builtinChip.Name}'...");
            loadedChips.Add(builtinChip.Name, builtinChip);
            await Task.Yield();
        }

        foreach (var chip in chipsToLoadDic)
        {
            progressBar.SetValue(i, $"Loading '{chip.Value.Info.name}'...");
            if (!loadedChips.ContainsKey(chip.Key))
            {
                try
                {
                    ResolveDependency(chip.Value);
                }
                catch (Exception e)
                {
                    DLSLogger.LogWarning($"Custom Chip '{chip.Value.Info.name}' could not be loaded!", e.ToString());
                }
            }

            await Task.Yield();
        }

        progressBar.SetValue(progressBar.progressBar.maxValue, "Done!");
        progressBar.Close();
        DLSLogger.Log($"Load time: {sw.ElapsedMilliseconds}ms");

        // the simulation will never create Cyclic path so simple recursive descending graph explore should be fine
        async void ResolveDependency(SavedChip chip)
        {
            foreach (var dependency in chip.ChipDependencies)
            {
                if (loadedChips.ContainsKey(dependency)) continue;

                ResolveDependency(chipsToLoadDic[dependency]);
                await Task.Yield();
                i++;
            }


            if (loadedChips.ContainsKey(chip.Info.name)) return;
            Chip loadedChip = manager.LoadCustomChip(LoadChip(chip, loadedChips));
            loadedChips.Add(loadedChip.Name, loadedChip);
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
            foreach (var dependency in chipToLoad.ChipDependencies)
            {
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

        DLSLogger.LogError($"Failed to load {chipToLoad.Info.name} sub component: {MissingComp} was missing");

        return null;
    }


    private static Dictionary<int,float> CalculateSignalGroupCenter(SavedChip chipToLoad)
    {
        var signalGroupCenter = new Dictionary<int,float>();

        var signal = chipToLoad.savedComponentChips.Where(x => x.chipName.Equals("SIGNAL IN")|| x.chipName.Equals("SIGNAL OUT")).ToList();

        foreach (var id in signal.Select(x => x.signalGroupId).Distinct())
        {
            var signalGroup = signal.Where(x => x.signalGroupId == id).Select(x => x.posY).ToList();
            var min = signalGroup.Min();
            var max = signalGroup.Max();
            signalGroupCenter.Add(id, (min + max) / 2);
        }


        return signalGroupCenter;
    }

    static ChipInstanceHolder LoadChipWithWires(SavedChip chipToLoad, Dictionary<string, Chip> loadedChips,ChipEditor chipEditor = null)
    {
        if (chipEditor is null)
            chipEditor = Manager.ActiveEditor;

        ChipInstanceHolder loadedChipData = new ChipInstanceHolder();
        int numComponents = chipToLoad.savedComponentChips.Length;
        loadedChipData.componentChips = new Chip[numComponents];
        loadedChipData.Info = chipToLoad.Info;
        List<Wire> loadedWires = new List<Wire>();


        // Spawn component chips (the chips used to create this chip)
        // These will have been loaded already, and stored in the
        // loadedChips dictionary
        for (int i = 0; i < numComponents; i++)
        {
            SavedComponentChip savedComponentToLoad = chipToLoad.savedComponentChips[i];
            string componentName = savedComponentToLoad.chipName;


            if (!loadedChips.ContainsKey(componentName))
                DLSLogger.LogError(
                    $"Failed to load sub component: {componentName} While loading {chipToLoad.Info.name}");


            Chip instanceComponent = chipEditor.LoadInstanceData(loadedChips[componentName],savedComponentToLoad);
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
            SavedComponentChip savedComponentChip = chipToLoad.savedComponentChips[chipIndex];

            for (int inputPinIndex = 0;
                 inputPinIndex < loadedComponentChip.inputPins.Count &&
                 inputPinIndex < savedComponentChip.inputPins.Length;
                 inputPinIndex++)
            {
                SavedInputPin savedPin = savedComponentChip.inputPins[inputPinIndex];
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
                loadedWires.Add(loadedWire);
            }
        }

        loadedChipData.wires = loadedWires.ToArray();



        chipEditor.SetSignalCenter(CalculateSignalGroupCenter(chipToLoad));


        return loadedChipData;
    }

    public static ChipInstanceHolder GetChipInstanceData(string chipname, ChipEditor chipEditor)
    {
        // @NOTE: chipEditor can be removed here if:
        //     * Chip & wire instatiation is inside their respective implementation
        //     holders is inside the chipEditor
        //     * the wire connections are done inside ChipEditor.LoadFromSaveData
        //     instead of ChipLoader.LoadChipWithWires

        SavedChip chipToTryLoad = SaveSystem.ReadChip(chipname);

        if (chipToTryLoad == null)
            return null;

        ChipInstanceHolder loadedChipData =LoadChipWithWires(chipToTryLoad, Manager.instance.AllSpawnableChipDic(), chipEditor);

        SetAnchorPoint(loadedChipData, chipToTryLoad.Connections);

        return loadedChipData;
    }

    private static void SetAnchorPoint(ChipInstanceHolder loadedChipData, SavedWire[] wireLayout)
    {
        // This fixes a bug where if a pin were unnamed, some connections were random.
        //Work Around solution. it just Work but maybe is worth to change the entire way to save wires (idk i don't think so)
        for (int i = 0; i < loadedChipData.wires.Length; i++)
        {
            Wire wire = loadedChipData.wires[i];
            wire.endPin.pinName += i;
        }

        // Set wires anchor points
        foreach (SavedWire wire in wireLayout)
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

            var wireIndex = Array.FindIndex(loadedChipData.wires,
                w => w.startPin.pinName == startPinName && w.endPin.pinName == endPinName);
            if (wireIndex < 0) continue;
            loadedChipData.wires[wireIndex].SetAnchorPoints(wire.anchorPoints);
            var wireDisplay = loadedChipData.wires[wireIndex].GetComponentInChildren<WireDisplay>();

            //The null check for 'wire.ThemeName' is redundant here since it's already handled in the method
            wireDisplay.SetThemeByName(wire.ColourThemeName);
        }

        foreach (var wire in loadedChipData.wires)
            wire.endPin.pinName = wire.endPin.pinName.Remove(wire.endPin.pinName.Length - 1);

    }

    public static void Import(string path)
    {
        //TODO: Need to be updated
    }
}