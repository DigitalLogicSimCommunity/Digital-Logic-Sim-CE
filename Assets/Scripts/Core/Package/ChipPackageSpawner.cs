using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core
{
    //class to handle chip spawning and configuration
    public class ChipPackageSpawner : MonoBehaviour
    {
        public static ChipPackageSpawner i;
        public ChipPackageDisplay chipPackageDisplayTemplate;
        public Pin chipPinPrefab;

        const string pinHolderName = "Pin Holder";

        private Transform PinHolder;

        private void Awake()
        {
            i = this;
        }

        #region BuiltIn

        public SpawnableChip GenerateBuiltInPackageAndChip<T>() where T : BuiltinChip
        {
            ChipPackageDisplay packageDisplay = IstansiatePackageTemplate();

            PackageBuiltinChip<T>();
            packageDisplay.gameObject.SetActive(false);

            var customChip = packageDisplay.GetComponent<SpawnableChip>();

            if (customChip is CustomChip c)
                c.Init();

            return customChip;
        }

        private void PackageBuiltinChip<T>() where T : BuiltinChip
        {
            ChipEditor chipEditor = Manager.ActiveEditor;
            var chipName = chipEditor.CurrentChip.name;
            // Add and set up the custom chip component
            var chip = gameObject.AddComponent<T>();

            chip.Name = chipName;

            // Create pins and set set package size
            SpawnPins(chip);
        }
        
        private void SpawnPins(BuiltinChip chip)
        {
            chip.inputPins = new List<Pin>(chip.inputPins.Count);
            chip.outputPins = new List<Pin>(chip.outputPins.Count);

            for (int i = 0; i < chip.inputPins.Count; i++)
            {
                Pin inputPin = Instantiate(chipPinPrefab, PinHolder.position, Quaternion.identity, PinHolder);
                inputPin.pinType = Pin.PinType.ChipInput;
                inputPin.chip = chip;
                inputPin.pinName = chip.inputPins[i].pinName;
                chip.inputPins[i] = inputPin;
            }

            for (int i = 0; i < chip.outputPins.Count; i++)
            {
                Pin outputPin = Instantiate(chipPinPrefab, PinHolder.position,
                    Quaternion.identity, PinHolder);
                outputPin.pinType = Pin.PinType.ChipOutput;
                outputPin.chip = chip;
                outputPin.pinName = chip.outputPins[i].pinName;
                chip.outputPins[i] = outputPin;
            }
        }

        #endregion BuiltIn


        public SpawnableChip GenerateCustomPackageAndChip()
        {
            ChipPackageDisplay packageDisplay = IstansiatePackageTemplate();
            packageDisplay.gameObject.SetActive(false);

            var customChip =  PackageCustomChip(packageDisplay);

            customChip.Init();
            return customChip;
        }

        private ChipPackageDisplay IstansiatePackageTemplate()
        {
            var package = Instantiate(chipPackageDisplayTemplate, transform);
            PinHolder = package.transform.GetChild(0);
            return package;
        }


        private CustomChip PackageCustomChip(ChipPackageDisplay packageDisplay)
        {
            ChipEditor chipEditor = Manager.ActiveEditor;
            
            // Add and set up the custom chip component
            var chip = SetUpChip(packageDisplay, chipEditor.CurrentChip);




            // Set input signals
            chip.inputSignals = chipEditor.InputSignals.ToArray();
            // Set output signals
            chip.outputSignals = chipEditor.OutputSignals.ToArray();


            // Create pins
            SpawnPins(chip);
            chipEditor.chipImplementationHolder.SetParent(packageDisplay.transform);
            chipEditor.chipImplementationHolder.gameObject.SetActive(false);
            packageDisplay.Init();
            
            return chip;
        }

        private static CustomChip SetUpChip(ChipPackageDisplay packageDisplay, ChipInfo chipInfo)
        {
            var chip = packageDisplay.gameObject.AddComponent<CustomChip>();

            chip.Name = chipInfo.name;
            chip.FolderIndex = chipInfo.FolderIndex;
            chip.PackageGraphicData = new PackageGraphicData()
            {
                PackageColour = chipInfo.PackColor,
                NameTextColor = chipInfo.PackNameColor
            };
            packageDisplay.SetUpForCustomPackageChip(chipInfo);
            return chip;
        }


        private void SpawnPins(CustomChip chip)
        {
            chip.inputPins = new List<Pin>(chip.inputSignals.Length);
            chip.outputPins = new List<Pin>(chip.outputSignals.Length);


            for (int i = 0; i < chip.inputPins.Capacity; i++)
            {
                Pin inputPin = Instantiate(chipPinPrefab, PinHolder.position, Quaternion.identity, PinHolder);
                inputPin.pinType = Pin.PinType.ChipInput;
                inputPin.chip = chip;
                inputPin.pinName = chip.inputSignals[i].outputPins[0].pinName;
                chip.inputPins.Add(inputPin);
            }

            for (int i = 0; i < chip.outputPins.Capacity; i++)
            {
                Pin outputPin = Instantiate(chipPinPrefab, PinHolder.position,
                    Quaternion.identity, PinHolder);
                outputPin.pinType = Pin.PinType.ChipOutput;
                outputPin.chip = chip;
                outputPin.pinName = chip.outputSignals[i].inputPins[0].pinName;
                chip.outputPins.Add(outputPin);
            }
        }

        
        public SpawnableChip TryPackageAndReplaceChip(List<SpawnableChip> SpawnableCustomChips ,string original)
        {
            ChipPackageDisplay oldPackageDisplay =GetComponentsInChildren<ChipPackageDisplay>(true).First( cp => cp.name == original);
            if (oldPackageDisplay is not null)
                Destroy(oldPackageDisplay.gameObject);


            var customChip = GenerateCustomPackageAndChip();

            int index = SpawnableCustomChips.FindIndex(c => c.Name == original);

            if (index < 0) return customChip;

            SpawnableCustomChips[index] = customChip;



            return customChip;
        }

    }
}