using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Linq;
using DLS.ChipData;
using DLS.SaveSystem.Serializable.SerializationHelper;
using Modules.ProjectSettings;
using Color = UnityEngine.Color;
using ColorConverter = DLS.SaveSystem.Serializable.SerializationHelper.ColorConverter;
using Debug = UnityEngine.Debug;


/// <summary>
/// this class have the role to ensre that the save file is compatible with the current version of the game by updating the save file to the format usade by this version of DLS
/// this is done in an incremental way meaning that one step is done to ensure compatibility with the next version of the game until the save file is compatible with the current version
/// </summary>
public class SaveCompatibility : MonoBehaviour
{
    private const bool WRITEENABLE = true;

    private static bool DirtyBit { get; set; } = false;
    private static bool Error { get; set; } = false;

    public static bool CanWriteFile => WRITEENABLE && DirtyBit &&!Error;

    private static bool FolderDirtyBit = false;

    private static string CurrentChipName;

    public delegate void CompDelegate(ref JObject SaveFile);


    public struct ChipInfov39
    {
        public string name;
        public int creationIndex;
        public Color Colour;
        public Color NameColour;
        public int FolderIndex;
        public float scale;

    }
    public struct ChipDatav38
    {
        public string name;
        public int creationIndex;
        public Color Colour;
        public Color NameColour;
        public string folderName;
        public float scale;
    }

    class OutputPin
    {
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("wireType")] public int wireType { get; set; }
    }

    class InputPin
    {
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("parentChipIndex")] public int parentChipIndex { get; set; }

        [JsonProperty("parentChipOutputIndex")]
        public int parentChipOutputIndex { get; set; }

        [JsonProperty("isCylic")] public bool isCylic { get; set; }
        [JsonProperty("wireType")] public int wireType { get; set; }
    }

    #region SaveFile

    public static SavedChip FixSaveCompatibility(string chipSaveStr, string chipName)
    {
        DirtyBit = false;
        Error = false;
        CurrentChipName = chipName;
        var JChipSave = JsonConvert.DeserializeObject(chipSaveStr) as JObject;

        if (!chipSaveStr.Contains("wireType") || chipSaveStr.Contains("outputPinNames") ||
            !chipSaveStr.Contains("outputPins"))
            CheckedCompatibility(From025to037, ref JChipSave, "25", "37");
        if (chipSaveStr.Contains("folderName") &&
            (!chipSaveStr.Contains("Data") || !chipSaveStr.Contains("ChipDependecies")))
            CheckedCompatibility(From037to038, ref JChipSave, "37", "38");
        if (chipSaveStr.Contains("folderName") || !chipSaveStr.Contains("FolderIndex"))
            CheckedCompatibility(From038to039, ref JChipSave, "38", "39");
        if (!chipSaveStr.Contains("Connections") || !chipSaveStr.Contains("Version"))
            CheckedCompatibility(From039to040, ref JChipSave, "39", "40");


        // if (!chipSaveStr.Contains("Connections"))
        //     UpdateToOfficial(ref JChipSave, SaveSystem.ReadWire(chipName), chipName);

        CurrentChipName = "";

        if (!DirtyBit )
            return null;

        var updateSaveContent = JsonConvert.SerializeObject(JChipSave, Formatting.Indented);
        return SaveSystem.DeserializeChip(updateSaveContent);

    }





    private static void CheckedCompatibility(CompDelegate CompDel, ref JObject JChipSave, string vfrom, string Vto)
    {
        try
        {
            CompDel(ref JChipSave);
        }
        catch
        {
            Error = true;
            DLSLogger.LogError($" failed to ensure compatibility of {CurrentChipName}", $"{vfrom} to {Vto} ");
            throw;
        }
    }



    private static void From025to037(ref JObject JChipSave)
    {
        var savedComponentChips = JChipSave.Property("savedComponentChips").Value as JArray;
        foreach (JToken SavedCompChip in savedComponentChips)
        {
            var CurrentComponent = SavedCompChip as JObject;
            List<OutputPin> newOutputPins = new List<OutputPin>();
            List<InputPin> newinputPins = new List<InputPin>();

            // Replace all 'outputPinNames' : [string] in save with 'outputPins' :
            // [OutputPin]
            var outputPinNames = (CurrentComponent.Property("outputPinNames").Value as JArray);
            for (int j = 0; j < outputPinNames.Count; j++)
            {
                var OutPutPinNameValue = (CurrentComponent.Property("outputPinNames").Value as JArray)[j];
                newOutputPins.Add(
                    new OutputPin
                    {
                        name = OutPutPinNameValue.ToString(),
                        wireType = 0
                    });
            }

            CurrentComponent.Property("outputPinNames").Remove();
            CurrentComponent.Add("outputPins", JArray.FromObject(newOutputPins));

            // Add to all 'inputPins' dictionary the property 'wireType' with a value
            // of 0 (at version 0.25 buses did not exist, it's impossible for the wire
            // to be of other type)
            var inputPins = CurrentComponent.Property("inputPins").Value as JArray;
            foreach (var jtok in inputPins)
            {
                var InPutPin = jtok as JObject;
                newinputPins.Add(new InputPin
                {
                    name = InPutPin.Property("name").Value.ToString(),
                    parentChipIndex = InPutPin.Property("parentChipIndex").Value.ToObject<int>(),
                    parentChipOutputIndex = InPutPin.Property("parentChipOutputIndex").Value.ToObject<int>(),
                    isCylic = InPutPin.Property("isCylic").Value.ToObject<bool>(),
                    wireType = 0
                });
            }

            CurrentComponent.Property("inputPins").Remove();
            CurrentComponent.Add("inputPins", JArray.FromObject(newinputPins));
        }

        JChipSave.Add("folderName", "User");
        JChipSave.Add("scale", 1);
        DirtyBit = true;
    }

    private static void From037to038(ref JObject JChipSave)
    {
        if (JChipSave.Property("Data") == null)
        {
            //replace old sparse data chip with new datachip
            var NewChipData = new ChipDatav38()
            {
                name = JChipSave.Property("name").Value.ToString(),
                creationIndex = JChipSave.Property("creationIndex").ToObject<int>(),
                Colour = JChipSave.Property("colour").Value.ToObject<Color>(),
                NameColour = JChipSave.Property("nameColour").Value.ToObject<Color>(),
                folderName = JChipSave.Property("folderName").Value.ToString(),
                scale = JChipSave.Property("scale").Value.ToObject<float>()
            };


            JChipSave.Add("Data", JObject.FromObject(NewChipData, ColorConverter.GenerateSerializerConverter()));


            JChipSave.Property("name").Remove();
            JChipSave.Property("creationIndex").Remove();
            JChipSave.Property("colour").Remove();
            JChipSave.Property("nameColour").Remove();
            JChipSave.Property("folderName").Remove();
            JChipSave.Property("scale").Remove();
        }

        //dont fix this typo
        if (JChipSave.Property("ChipDependecies") == null && JChipSave.Property("componentNameList") != null)
        {
            JChipSave.Add("ChipDependecies", JChipSave.Property("componentNameList").Value);
            JChipSave.Property("componentNameList").Remove();
        }

        DirtyBit = true;
    }

    private static void From038to039(ref JObject JChipSave)
    {

        var OldData = JChipSave.Property("Data").Value as JObject;

        var Colour = JsonConvert.DeserializeObject<JObject>(OldData.Property("Colour").Value.ToString());
        var NameColour = JsonConvert.DeserializeObject<JObject>(OldData.Property("NameColour").Value.ToString());
        int folder = OldData.Property("folderName") == null
            ? -1
            : ProjectSettings.FolderSystem.ReverseIndex(OldData.Property("folderName").Value.ToString());
        float scale = OldData.Property("scale") == null ? 1 : OldData.Property("scale").Value.ToObject<float>();

        var NewChipData = new ChipInfov39()
        {
            name = OldData.Property("name").Value.ToString(),
            creationIndex = OldData.Property("creationIndex").Value.ToObject<int>(),
            Colour = Colour.ToObject<Color>(),
            NameColour = NameColour.ToObject<Color>(),
            FolderIndex = folder,
            scale = scale
        };


        JChipSave.Property("Data").Value =
            JObject.FromObject(NewChipData, ColorConverter.GenerateSerializerConverter());

        DirtyBit = true;
    }

    private static void From039to040(ref JObject JChipSave)
    {


        JChipSave.Add("Version", "0.40-CE");

        //Rewrite Color in HEX
        var OldData = JChipSave.Property("Data").Value as JObject;

        var packColor = JsonConvert.DeserializeObject<JObject>(OldData.Property("Colour").Value.ToString());
        var nameColor = JsonConvert.DeserializeObject<JObject>(OldData.Property("NameColour").Value.ToString());

        var NewChipInfo = new ChipInfo()
        {
            name = OldData.Property("name").Value.ToString(),
            PackColor = packColor.ToObject<Color>(),
            PackNameColor = nameColor.ToObject<Color>(),
            FolderIndex = OldData.Property("FolderIndex").Value.ToObject<int>(),
            scale = OldData.Property("scale").Value.ToObject<float>()
        };


        JChipSave.Remove("Data");

        JChipSave.Add("Info", JObject.FromObject(NewChipInfo, ColorConverterHEX.GenerateSerializerConverter()));


        //Remove the SIGNAL IN and SIGNAL OUT from the ChipDependencies and fix typo
        var OldChipDependecies = JChipSave.Property("ChipDependecies").Value as JArray;

        var oldcdtemp = new List<string>(JsonConvert.DeserializeObject<string[]>(OldChipDependecies.ToString()));

        var NewChipData = oldcdtemp.SkipWhile(x => x.Equals("SIGNAL IN") || x.Equals("SIGNAL OUT")).ToArray();

        //don't fix this typo
        JChipSave.Property("ChipDependecies").Remove();
        var newChipDependencies = JArray.FromObject(NewChipData);
        JChipSave.Add("ChipDependencies", newChipDependencies);


        SaveSystem.FileExtension= ".txt";
        SaveSystem.ChipFolder = "";
        //Merge ChipData and WireData
        SavedWireLayout wireLayout = SaveSystem.Legacy.ReadWireLayout(CurrentChipName);
        SaveSystem.ResetToDefaultSettings();

        JChipSave.Add("Connections", JArray.FromObject(wireLayout.serializableWires));

        DirtyBit = true;
    }



    private static void GenerateConnection(ChipDescription chipDescription, SavedComponentChip[] subComponentsOld,
        SavedWireLayout wireLayout)
    {
        var connectionLs = new List<ConnectionDescription>();
        var inputSignalNumberOfChip = subComponentsOld.Where(x => x.chipName.Equals("SIGNAL IN")).ToArray().Length;
        foreach (var subCom in subComponentsOld)
        {
            foreach (var subOutPin in subCom.inputPins)
            {
                if (subOutPin.parentChipOutputIndex == -1) continue;
                var connection = new ConnectionDescription();
                connection.Source = new PinAddress();
                connection.Source.PinType = subOutPin.parentChipIndex < chipDescription.InputPins.Length
                    ? PinType.ChipInputPin
                    : PinType.SubChipOutputPin;


                if (connection.Source.PinType == PinType.ChipInputPin)
                {
                    connection.Source.SubChipID = 0;
                    connection.Source.PinID = subOutPin.parentChipIndex;
                }
                else
                {
                    connection.Source.SubChipID = subOutPin.parentChipIndex;
                    connection.Source.PinID = subOutPin.parentChipOutputIndex;
                }

                SavedWire wire = GetSavedWire(wireLayout, subOutPin.parentChipIndex, subOutPin.parentChipOutputIndex);

                connection.Target = new PinAddress();
                connection.Target.PinType = wire.childChipIndex <
                                            chipDescription.InputPins.Length + chipDescription.OutputPins.Length
                    ? PinType.ChipOutputPin
                    : PinType.SubChipInputPin;

                if (connection.Target.PinType == PinType.ChipOutputPin)
                {
                    connection.Target.SubChipID = 0;
                    connection.Target.PinID = wire.childChipIndex;
                }
                else
                {
                    connection.Target.SubChipID = wire.childChipIndex;
                    connection.Target.PinID = wire.childChipInputIndex;
                }

                connection.ColourThemeName = ThemeManager.DefaultTheme.Name;
                connection.WirePoints = wire.anchorPoints.Select(x => new Point(x.x, x.y)).ToArray();
                connectionLs.Add(connection);
            }
        }

        chipDescription.Connections = connectionLs.ToArray();
    }

    private static SavedWire GetSavedWire(SavedWireLayout wireLayout, int ChipID, int pinID)
    {
        return wireLayout.serializableWires.FirstOrDefault(wire =>
            wire.parentChipIndex == ChipID && wire.parentChipOutputIndex == pinID);
    }


    private static void GenerateSubComponent(ChipDescription chipDescription, SavedComponentChip[] subComponentsOld)
    {
        List<SignalDescription> inputSignal = new List<SignalDescription>();
        List<SignalDescription> outputSignal = new List<SignalDescription>();
        List<ChipInstanceData> chipInstanceData = new List<ChipInstanceData>();

        for (var index = 0; index < subComponentsOld.Length; index++)
        {
            var subCom = subComponentsOld[index];
            switch (subCom.chipName)
            {
                case "SIGNAL IN":
                {
                    var signalDescription = new SignalDescription
                    {
                        Name = subCom.outputPins[0].name,
                        PositionY = subCom.posY,
                        ID = index,
                        GroupID = subCom.signalGroupId,
                        ColourThemeName = ThemeManager.DefaultTheme.Name,
                        WireType = subCom.outputPins[0].wireType
                    };
                    inputSignal.Add(signalDescription);
                    break;
                }
                case "SIGNAL OUT":
                {
                    var signalDescription = new SignalDescription
                    {
                        Name = subCom.inputPins[0].name,
                        PositionY = subCom.posY,
                        ID = index,
                        GroupID = subCom.signalGroupId,
                        ColourThemeName = ThemeManager.DefaultTheme.Name,
                        WireType = subCom.inputPins[0].wireType
                    };
                    outputSignal.Add(signalDescription);
                    break;
                }
                default:
                {
                    var points = new Point[1];
                    points[0] = new Point(subCom.posX, subCom.posY);
                    ChipInstanceData instanceData = new ChipInstanceData()
                    {
                        Name = subCom.chipName,
                        ID = index,
                        Points = points,
                        Data = null
                    };

                    chipInstanceData.Add(instanceData);
                    break;
                }
            }
        }

        chipDescription.InputPins = inputSignal.ToArray();
        chipDescription.SubChips = chipInstanceData.ToArray();
        chipDescription.OutputPins = outputSignal.ToArray();
    }

    #endregion

    #region FolderFile

    public static void FixFolderCompatibility(ref string FolderFile)
    {
        var FolderJson = JsonConvert.DeserializeObject(FolderFile) as JObject;


        if (FolderJson.Property("0") == null)
            From038to039Folder(ref FolderJson);

        if (!FolderDirtyBit) return;

        FolderFile = JsonConvert.SerializeObject(FolderJson);
        WriteFileFolder(FolderFile);
    }

    private static void WriteFileFolder(string FoldersJsonStr)
    {
        if (!FolderDirtyBit && !WRITEENABLE) return;
        SaveSystem.WriteProjectSettings(FoldersJsonStr);
        DirtyBit = false;
    }

    private static void From038to039Folder(ref JObject folderJson)
    {
        JObject NewFolderFileTemp = new JObject();

        foreach (var item in folderJson.Properties())
        {
            NewFolderFileTemp.Add(item.Value.ToString(), item.Name);
        }

        folderJson.RemoveAll();
        foreach (var item in NewFolderFileTemp.Properties())
        {
            folderJson.Add(item.Name, item.Value);
        }

        FolderDirtyBit = true;
    }

    #endregion
}