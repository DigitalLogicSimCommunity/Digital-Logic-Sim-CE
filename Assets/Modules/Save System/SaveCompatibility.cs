using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System;

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
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("wireType")]
        public int wireType { get; set; }
    }

    class InputPin
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("parentChipIndex")]
        public int parentChipIndex { get; set; }
        [JsonProperty("parentChipOutputIndex")]
        public int parentChipOutputIndex { get; set; }
        [JsonProperty("isCylic")]
        public bool isCylic { get; set; }
        [JsonProperty("wireType")]
        public int wireType { get; set; }
    }

    #region SaveFile
    public static void FixSaveCompatibility(ref string chipSaveString)
    {

        var lol = JsonConvert.DeserializeObject(chipSaveString) as JObject;

        if (!chipSaveString.Contains("wireType") || chipSaveString.Contains("outputPinNames") || !chipSaveString.Contains("outputPins"))
            CheckedCompatibility(From025to037, ref lol, "25", "37");
        if (!chipSaveString.Contains("Data") || !chipSaveString.Contains("ChipDependecies"))
            CheckedCompatibility(From037to038, ref lol, "37", "38");
        if (chipSaveString.Contains("folderName") || !chipSaveString.Contains("FolderIndex"))
            CheckedCompatibility(From038to039, ref lol, "38", "39");

        if (DirtyBit)
        {
            chipSaveString = JsonConvert.SerializeObject(lol, Formatting.Indented);
            WriteFile((lol.Property("Data").Value as JObject).Property("name").Value.ToString(), chipSaveString);
        }
    }

    private static void CheckedCompatibility(CompDelegate CompDel, ref JObject lol, string vfrom, string Vto)
    {
        try
        {
            CompDel(ref lol);
        }
        catch
        {
            string name = "";
            var Name = lol.Property("name");

            if (Name != null)
                name = Name.Value.ToString();
            else
            {
                var DataName = (lol.Property("Data").Value as JObject).Property("name");
                if (DataName != null)
                    name = DataName.Value.ToString();
            }
            DLSLogger.LogError($" failed to ensure compatibility of {name}", $"{vfrom} to {Vto} ");
        }
    }

    private static void WriteFile(string Chipname, string ChipContent)
    {
        if (!DirtyBit || !WRITEENABLE) return;
        SaveSystem.WriteChip(Chipname, ChipContent);
        DirtyBit = false;
    }

    private static void From025to037(ref JObject lol)
    {
        var savedComponentChips = lol.Property("savedComponentChips").Value as JArray;
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
            for (int j = 0; j < inputPins.Count; j++)
            {
                var InPutPin = inputPins[j] as JObject;
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
        lol.Add("folderName", "User");
        lol.Add("scale", 1);
        DirtyBit = true;
    }

    private static void From037to038(ref JObject lol)
    {
        if (lol.Property("Data") == null)
        {
            //replace old sparse data chip with new datachip
            var NewChipData = new ChipDataComp()
            {
                name = lol.Property("name").Value.ToString(),
                creationIndex = lol.Property("creationIndex").ToObject<int>(),
                Colour = lol.Property("colour").Value.ToObject<Color>(),
                NameColour = lol.Property("nameColour").Value.ToObject<Color>(),
                folderName = lol.Property("folderName").Value.ToString(),
                scale = lol.Property("scale").Value.ToObject<float>()
            };


            lol.Add("Data", JObject.FromObject(NewChipData, ColorConverter.GenerateSerializerConverter()));
            //lol.Add("Data", JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(NewChipData, ColorConverter.GenerateSettingsConverterForColor())));

            lol.Property("name").Remove();
            lol.Property("creationIndex").Remove();
            lol.Property("colour").Remove();
            lol.Property("nameColour").Remove();
            lol.Property("folderName").Remove();
            lol.Property("scale").Remove();
        }

        if (lol.Property("ChipDependecies") == null && lol.Property("componentNameList") != null)
        {
            lol.Add("ChipDependecies", lol.Property("componentNameList").Value);
            lol.Property("componentNameList").Remove();
        }

        DirtyBit = true;
    }

    private static void From038to039(ref JObject lol)
    {
        var OldData = lol.Property("Data").Value as JObject;

        var Colour = JsonConvert.DeserializeObject<JObject>(OldData.Property("Colour").Value.ToString());
        var NameColour = JsonConvert.DeserializeObject<JObject>(OldData.Property("NameColour").Value.ToString());
        int folder = OldData.Property("folderName") == null ? -1 : FolderSystem.ReverseIndex(OldData.Property("folderName").Value.ToString());
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




        lol.Property("Data").Value = JObject.FromObject(NewChipData, ColorConverter.GenerateSerializerConverter());

        DirtyBit = true;

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
