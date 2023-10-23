using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using DLS.ChipData;
using UnityEditor.MemoryProfiler;
using Color = UnityEngine.Color;
using ColorConverter = DLS.SaveSystem.Serializable.SerializationHelper.ColorConverter;

public class SaveCompatibility : MonoBehaviour
{
    private const bool WRITEENABLE = true;

    private static bool DirtyBit = false;

    private static bool FolderDirtyBit = false;

    public delegate void CompDelegate(ref JObject SaveFile);


    public struct ChipDataComp
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

    public static void FixSaveCompatibility(ref string chipSaveStr)
    {
        var JChipSave = JsonConvert.DeserializeObject(chipSaveStr) as JObject;

        if (!chipSaveStr.Contains("wireType") || chipSaveStr.Contains("outputPinNames") ||
            !chipSaveStr.Contains("outputPins"))
            CheckedCompatibility(From025to037, ref JChipSave, "25", "37");
        if (!chipSaveStr.Contains("Data") || !chipSaveStr.Contains("ChipDependecies"))
            CheckedCompatibility(From037to038, ref JChipSave, "37", "38");
        if (chipSaveStr.Contains("folderName") || !chipSaveStr.Contains("FolderIndex"))
            CheckedCompatibility(From038to039, ref JChipSave, "38", "39");

        string chipName = (JChipSave.Property("Data").Value as JObject).Property("name").Value.ToString();


        if (!chipSaveStr.Contains("Connections"))
            UpdateToOfficial(ref JChipSave, SaveSystem.ReadWire(chipName), chipName);

        if (!DirtyBit) return;

        chipSaveStr = JsonConvert.SerializeObject(JChipSave, Formatting.Indented);
        WriteFile(chipName, chipSaveStr);
    }


    private static void UpdateToOfficial(ref JObject JChipSave, SavedWireLayout wireSaveStr, string name)
    {
        var ConnectionSave = wireSaveStr;

        try
        {
            From039ToOfficial(ref JChipSave, ConnectionSave);

            //Debug
            SaveSystem.SetExtension(".json");
            var Content = JsonConvert.SerializeObject(JChipSave, Formatting.Indented);
            WriteFile(name, Content, "\\Official");
            SaveSystem.SetExtension(".txt");
            //EndDebug
        }
        catch (Exception e)
        {
            DLSLogger.LogError($" failed to ensure compatibility of {name}", "039 to Official " + e.Message);
        }
    }


    private static void CheckedCompatibility(CompDelegate CompDel, ref JObject JChipSave, string vfrom, string Vto)
    {
        try
        {
            CompDel(ref JChipSave);
        }
        catch
        {
            string name = "";
            var Name = JChipSave.Property("name");

            if (Name != null)
                name = Name.Value.ToString();
            else
            {
                var DataName = (JChipSave.Property("Data").Value as JObject).Property("name");
                if (DataName != null)
                    name = DataName.Value.ToString();
            }

            DLSLogger.LogError($" failed to ensure compatibility of {name}", $"{vfrom} to {Vto} ");
        }
    }

    private static void WriteFile(string Chipname, string ChipContent, string extraPath = "")
    {
        if (!DirtyBit || !WRITEENABLE) return;
        SaveSystem.WriteChip(Chipname, ChipContent, extraPath);
        DirtyBit = false;
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
            // of 0 (at version 0.25 buses did not exist so its imposible for the wire
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
            var NewChipData = new ChipDataComp()
            {
                name = JChipSave.Property("name").Value.ToString(),
                creationIndex = JChipSave.Property("creationIndex").ToObject<int>(),
                Colour = JChipSave.Property("colour").Value.ToObject<Color>(),
                NameColour = JChipSave.Property("nameColour").Value.ToObject<Color>(),
                folderName = JChipSave.Property("folderName").Value.ToString(),
                scale = JChipSave.Property("scale").Value.ToObject<float>()
            };


            JChipSave.Add("Data", JObject.FromObject(NewChipData, ColorConverter.GenerateSerializerConverter()));
            //lol.Add("Data", JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(NewChipData, ColorConverter.GenerateSettingsConverterForColor())));

            JChipSave.Property("name").Remove();
            JChipSave.Property("creationIndex").Remove();
            JChipSave.Property("colour").Remove();
            JChipSave.Property("nameColour").Remove();
            JChipSave.Property("folderName").Remove();
            JChipSave.Property("scale").Remove();
        }

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
            : FolderSystem.ReverseIndex(OldData.Property("folderName").Value.ToString());
        float scale = OldData.Property("scale") == null ? 1 : OldData.Property("scale").Value.ToObject<float>();

        var NewChipData = new ChipData()
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


    private static void From039ToOfficial(ref JObject JChipSave, SavedWireLayout wireSaveLayout)
    {
        ChipDescription NewSaveFormat = new ChipDescription();
        var OldData = JChipSave.Property("Data").Value as JObject;

        var Colour = JsonConvert.DeserializeObject<JObject>(OldData.Property("Colour").Value.ToString())
            .ToObject<Color>();
        var ColorHex = "#" + ColorUtility.ToHtmlStringRGB(Colour);
        int folderIndex = OldData.Property("folderName") == null
            ? -1
            : FolderSystem.ReverseIndex(OldData.Property("folderName").Value.ToString());
        float scale = OldData.Property("scale") == null ? 1 : OldData.Property("scale").Value.ToObject<float>();


        var SavedconpArray = JChipSave.Property("savedComponentChips").Value as JArray;
        List<SavedComponentChip> subComponentOld = new List<SavedComponentChip>();

        foreach (var con in SavedconpArray)
        {
            if (con is not JObject r) continue;
            var scc = new SavedComponentChip();

            var InPinLs = new List<SavedInputPin>();
            var inpins = r.Property("inputPins").Value as JArray;
            foreach (var jTokeninpin in inpins)
            {
                if (jTokeninpin is not JObject jObjectin) continue;
                var e = new SavedInputPin();

                e.name = jObjectin.Property("name").HasValues ? jObjectin.Property("name").Value.ToString() : "";
                e.parentChipIndex = jObjectin.Property("parentChipIndex").Value.ToObject<int>();
                e.parentChipOutputIndex = jObjectin.Property("parentChipOutputIndex").Value.ToObject<int>();
                e.isCylic = jObjectin.Property("isCylic").Value.ToObject<bool>();
                e.wireType = jObjectin.Property("wireType").Value.ToObject<Pin.WireType>();
                InPinLs.Add(jTokeninpin.ToObject<SavedInputPin>());
            }

            scc.inputPins = InPinLs.ToArray();


            var outPinLs = new List<SavedOutputPin>();
            var outpins = r.Property("outputPins").Value as JArray;

            foreach (var jTokenoupin in outpins)
            {
                if (jTokenoupin is not JObject jObjectout) continue;

                var savedOut = new SavedOutputPin
                {
                    name = jObjectout.Property("name").HasValues ? jObjectout.Property("name").Value.ToString() : "",
                    wireType = jObjectout.Property("wireType").Value.ToObject<Pin.WireType>()
                };
                outPinLs.Add(savedOut);
            }

            scc.outputPins = outPinLs.ToArray();


            scc.chipName = r.Property("chipName").Value.ToString();
            scc.posX = r.Property("posX").Value.ToObject<float>();
            scc.posY = r.Property("posY").Value.ToObject<float>();
            subComponentOld.Add(scc);
        }

        NewSaveFormat.Version = "CE-0.40";
        NewSaveFormat.Name = OldData.Property("name").Value.ToString();
        NewSaveFormat.Colour = ColorHex;
        NewSaveFormat.FolderIndex = folderIndex;
        NewSaveFormat.Scale = scale;
        GenerateSubComponent(NewSaveFormat, subComponentOld.ToArray());
        GenerateConnection(NewSaveFormat, subComponentOld.ToArray(), wireSaveLayout);
        NewSaveFormat.ChipDependencies = JChipSave.Property("ChipDependecies").Value.ToObject<string[]>()
            .Where(x => !(x.Equals("SIGNAL OUT") || x.Equals("SIGNAL IN"))).ToArray();

        JChipSave = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(NewSaveFormat)) as JObject;
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

                SavedWire wire = GetSavedWire(wireLayout,  subOutPin.parentChipIndex, subOutPin.parentChipOutputIndex);

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

        if (FolderDirtyBit)
        {
            FolderFile = JsonConvert.SerializeObject(FolderJson);
            WriteFileFolder(FolderFile);
        }
    }

    private static void WriteFileFolder(string FoldersJsonStr)
    {
        if (!FolderDirtyBit && !WRITEENABLE) return;
        SaveSystem.WriteFoldersFile(FoldersJsonStr);
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