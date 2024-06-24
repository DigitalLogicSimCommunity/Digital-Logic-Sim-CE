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
        public static void Save(ChipEditor chipEditor)
        {
            ChipInstanceHolder chipInstanceHolder = new ChipInstanceHolder(chipEditor);

            var compositeChip = new SavedChip(chipInstanceHolder);

            // Write to file
            SaveSystem.SaveChip(compositeChip.Info.name, compositeChip);

        }

        public static void Export(Chip exportedChip, string destinationPath)
        {
            //TODO: this need to be redone
        }

        static Dictionary<string, string> FindChildrenChips(string chipName)
        {
            //TODO: this need to be redone
            Dictionary<string, string> childrenChips = new Dictionary<string, string>();
            return childrenChips;

        }

        public static void Update(ChipEditor chipEditor, Chip chip)
        {
            ChipInstanceHolder chipInstanceHolder = new ChipInstanceHolder(chipEditor);

            // Write to file
            SaveSystem.SaveChip(chipEditor.CurrentChip.name, new SavedChip(chipInstanceHolder));


            // Update parent chips using this chip
            string currentChipName = chipEditor.CurrentChip.name;
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (var t in savedChips)
            {
                if (!t.ChipDependencies.Contains(currentChipName)) continue;
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
                SaveSystem.SaveChip(t.Info.name, t);
            }
        }

        internal static void ChangeFolder(string Chipname, int FolderIndex)
        {
            var ChipToEdit = SaveSystem.GetAllSavedChipsDic()[Chipname];
            if (ChipToEdit.Info.FolderIndex == FolderIndex) return;
            ChipToEdit.Info.FolderIndex = FolderIndex;
            SaveSystem.SaveChip(Chipname, ChipToEdit);
        }


        public static bool IsSafeToDelete(string chipName)
        {
            if (Manager.instance.AllChipNames(true, false).Contains(chipName))
                return false;

            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (SavedChip savedChip in savedChips)
                if (savedChip.ChipDependencies.Contains(chipName))
                    return false;
            return true;
        }


        public static void Delete(string chipName)
        {
            SaveSystem.DeleteChip(chipName);
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
                if (savedChip.Info.name == oldChipName)
                {
                    savedChip.Info.name = newChipName;
                    changed = true;
                }

                for (int j = 0; j < savedChip.ChipDependencies.Length; j++)
                {
                    string componentName = savedChip.ChipDependencies[j];
                    if (componentName != oldChipName) continue;
                    savedChip.ChipDependencies[j] = newChipName;
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
                // Write to file
                SaveSystem.SaveChip(savedChip.Info.name, savedChip);
            }

            SaveSystem.DeleteChip(oldChipName);
        }
    }
}