using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DLS.SaveSystem.Serializable.SerializationHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using ColorConverter = Newtonsoft.Json.Converters.ColorConverter;

namespace Modules.Save_System.Save
{
    public static class ChipSaver
    {
        const bool usePrettyPrint = true;

        public static void Save(ChipEditor chipEditor)
        {
            ChipInstanceHolder chipInstanceHolder = new ChipInstanceHolder(chipEditor);

            // Generate new chip save string
            var compositeChip = new SavedChip(chipInstanceHolder);
            string saveString = JsonUtility.ToJson(compositeChip, usePrettyPrint);

            // Generate save string for wire layout
            var wiringSystem = new SavedWireLayout(chipInstanceHolder);
            string wiringSaveString = JsonUtility.ToJson(wiringSystem, usePrettyPrint);

            // Write to file
            string savePath = SaveSystem.GetPathToSaveFile(chipEditor.Data.name);
            SaveSystem.WriteFile(savePath, saveString);

            string wireLayoutSavePath =
                SaveSystem.GetPathToWireSaveFile(chipEditor.Data.name);
            SaveSystem.WriteFile(wireLayoutSavePath, wiringSaveString);
        }

        public static void Export(Chip exportedChip, string destinationPath)
        {
            Dictionary<int, string> chipsToExport =
                FindChildrenChips(exportedChip.chipName);

            using StreamWriter writer = new StreamWriter(destinationPath);
            writer.WriteLine(chipsToExport.Count);

            foreach (KeyValuePair<int, string> chip in chipsToExport.OrderBy(x => x.Key))
            {
                string chipSaveFile = SaveSystem.GetPathToSaveFile(chip.Value);
                string chipWireSaveFile = SaveSystem.GetPathToWireSaveFile(chip.Value);

                using StreamReader reader = new StreamReader(chipSaveFile);
                string saveString = reader.ReadToEnd();

                using StreamReader wireReader = new StreamReader(chipWireSaveFile);
                string wiringSaveString = wireReader.ReadToEnd();

                writer.WriteLine(chip.Value);
                writer.WriteLine(saveString.Split('\n').Length);
                writer.WriteLine(wiringSaveString.Split('\n').Length);
                writer.WriteLine(saveString);
                writer.WriteLine(wiringSaveString);
            }
        }

        static Dictionary<int, string> FindChildrenChips(string chipName)
        {
            Dictionary<int, string> childrenChips = new Dictionary<int, string>();

            Manager manager = GameObject.FindObjectOfType<Manager>();
            SavedChip[] allChips = SaveSystem.GetAllSavedChips();
            SavedChip currentChip = Array.Find(allChips, c => c.Data.name == chipName);
            if (currentChip == null) return childrenChips;

            childrenChips.Add(currentChip.Data.creationIndex, chipName);

            foreach (SavedComponentChip scc in currentChip.savedComponentChips)
            {
                if (Array.FindIndex(manager.SpawnableBuiltinChips,
                        c => c.chipName == scc.chipName) != -1) continue;

                foreach (var chip in FindChildrenChips(scc.chipName).Where(chip => !childrenChips.ContainsKey(chip.Key)))
                {
                    childrenChips.Add(chip.Key, chip.Value);
                }
            }

            return childrenChips;
        }

        public static void Update(ChipEditor chipEditor, Chip chip)
        {
            ChipInstanceHolder chipInstanceHolder = new ChipInstanceHolder(chipEditor);



            // Generate new chip save string
            string saveString = JsonConvert.SerializeObject(new SavedChip(chipInstanceHolder),Formatting.Indented, JsonSerializationHelper.GetSettingsColorAndSkip());

            // Generate save string for wire layout
            string wiringSaveString = JsonUtility.ToJson(new SavedWireLayout(chipInstanceHolder), usePrettyPrint);

            // Write to file
            SaveSystem.WriteChip(chipEditor.Data.name, saveString);
            SaveSystem.WriteWire(chipEditor.Data.name, wiringSaveString);


            // Update parent chips using this chip
            string currentChipName = chipEditor.Data.name;
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (var t in savedChips)
            {
                if (!t.ChipDependecies.Contains(currentChipName)) continue;
                int currentChipIndex =
                    Array.FindIndex(t.savedComponentChips,
                        c => c.chipName == currentChipName);
                SavedComponentChip updatedComponentChip = new SavedComponentChip(chipInstanceHolder, chip);
                SavedComponentChip oldComponentChip =
                    t.savedComponentChips[currentChipIndex];

                // Update component chip I/O
                foreach (var updateInputPin in updatedComponentChip.inputPins)
                {
                    foreach (var oldInputPin in oldComponentChip.inputPins)
                    {
                        if (updateInputPin.name != oldInputPin.name) continue;
                        updateInputPin.parentChipIndex = oldInputPin.parentChipIndex;
                        updateInputPin.parentChipOutputIndex = oldInputPin.parentChipOutputIndex;
                        updateInputPin.isCylic = oldInputPin.isCylic;
                    }
                }

                // Write to file
                SaveSystem.WriteChip(t.Data.name, JsonUtility.ToJson(t, usePrettyPrint));
            }
        }

        internal static void ChangeFolder(string Chipname, int FolderIndex)
        {
            var ChipToEdit = SaveSystem.GetAllSavedChipsDic()[Chipname];
            if (ChipToEdit.Data.FolderIndex == FolderIndex) return;
            ChipToEdit.Data.FolderIndex = FolderIndex;
            SaveSystem.WriteChip(Chipname, JsonUtility.ToJson(ChipToEdit, usePrettyPrint));
        }

        public static void EditSavedChip(SavedChip savedChip, ChipInstanceHolder chipInstanceHolder)
        {
        }

        public static bool IsSafeToDelete(string chipName)
        {
            if (Manager.instance.AllChipNames(true, false).Contains(chipName))
                return false;

            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (SavedChip savedChip in savedChips)
                if (savedChip.ChipDependecies.Contains(chipName))
                    return false;
            return true;
        }

        public static bool IsSignalSafeToDelete(string chipName, string signalName)
        {
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (var t in savedChips)
            {
                if (!t.ChipDependecies.Contains(chipName)) continue;
                SavedChip parentChip = t;
                int currentChipIndex = Array.FindIndex(parentChip.savedComponentChips,
                    scc => scc.chipName == chipName);
                SavedComponentChip currentChip =
                    parentChip.savedComponentChips[currentChipIndex];
                int currentSignalIndex = Array.FindIndex(
                    currentChip.outputPins, name => name.name == signalName);

                if (Array.Find(currentChip.inputPins,
                        pin => pin.name == signalName && pin.parentChipIndex >= 0) != null)
                {
                    return false;
                }
                else if (currentSignalIndex >= 0 &&
                         parentChip.savedComponentChips.Any(scc => scc.inputPins.Any(pin =>
                             pin.parentChipIndex == currentChipIndex
                             && pin.parentChipOutputIndex == currentSignalIndex)))
                {
                    return false;
                }
            }

            return true;
        }

        public static void Delete(string chipName)
        {
            File.Delete(SaveSystem.GetPathToSaveFile(chipName));
            File.Delete(SaveSystem.GetPathToWireSaveFile(chipName));
        }

        public static void Rename(string oldChipName, string newChipName)
        {
            if (oldChipName == newChipName)
            {
                return;
            }

            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (var savedChip in savedChips)
            {
                bool changed = false;
                if (savedChip.Data.name == oldChipName)
                {
                    savedChip.Data.name = newChipName;
                    changed = true;
                }

                for (int j = 0; j < savedChip.ChipDependecies.Length; j++)
                {
                    string componentName = savedChip.ChipDependecies[j];
                    if (componentName != oldChipName) continue;
                    savedChip.ChipDependecies[j] = newChipName;
                    changed = true;
                }

                foreach (var savedComponent in savedChip.savedComponentChips)
                {
                    string componentChipName =
                        savedComponent.chipName;
                    if (componentChipName != oldChipName) continue;
                    savedComponent.chipName = newChipName;
                    changed = true;
                }

                if (!changed) continue;
                string saveString = JsonUtility.ToJson(savedChip, usePrettyPrint);
                // Write to file
                SaveSystem.WriteChip(savedChip.Data.name, saveString);
            }

            // Rename wire layer file
            string oldWireSaveFile = SaveSystem.GetPathToWireSaveFile(oldChipName);
            string newWireSaveFile = SaveSystem.GetPathToWireSaveFile(newChipName);
            try
            {
                System.IO.File.Move(oldWireSaveFile, newWireSaveFile);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            // Delete old chip save file
            File.Delete(SaveSystem.GetPathToSaveFile(oldChipName));
        }
    }
}