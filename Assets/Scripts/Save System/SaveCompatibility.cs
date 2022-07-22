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

    static bool DirtyBit = false;
    public static string FixStr(string message, char c)
    {
        // To remove double quotes (passed as 'c') of the chip name
        StringBuilder aStr = new StringBuilder(message);
        for (int i = 0; i < aStr.Length; i++)
            if (aStr[i] == c) aStr.Remove(i, 1);
        return aStr.ToString();
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

    public static void FixSaveCompatibility(ref string chipSaveString)
    {

        dynamic lol = JsonConvert.DeserializeObject<dynamic>(chipSaveString);

        if (!chipSaveString.Contains("wireType") || chipSaveString.Contains("outputPinNames") || !chipSaveString.Contains("outputPins"))
            From025to037(lol);
        if (!chipSaveString.Contains("Data") || !chipSaveString.Contains("ChipDependecies"))
            From037to038(lol);


        if (DirtyBit)
            chipSaveString = JsonConvert.SerializeObject(lol);

        WriteFile(lol, lol.Data.name);
    }



    private static void WriteFile(dynamic lol, dynamic Filename)
    {
        if (!DirtyBit) return;
        string savePath = SaveSystem.GetPathToSaveFile(FixStr(JsonConvert.SerializeObject(Filename), (char)0x22));
        string serialized = JsonConvert.SerializeObject(lol, Formatting.Indented);
        SaveSystem.WriteFile(savePath, serialized);
        DirtyBit = false;
    }

    private static void From025to037(dynamic lol)
    {
        for (int i = 0; i < lol.savedComponentChips.Count; i++)
        {

            List<OutputPin> newValue = new List<OutputPin>();
            List<InputPin> newValue2 = new List<InputPin>();

            // Replace all 'outputPinNames' : [string] in save with 'outputPins' :
            // [OutputPin]
            for (int j = 0; j < lol.savedComponentChips[i].outputPinNames.Count;
                 j++)
            {
                newValue.Add(
                    new OutputPin
                    {
                        name = lol.savedComponentChips[i].outputPinNames[j],
                        wireType = 0
                    });
            }
            lol.savedComponentChips[i].Property("outputPinNames").Remove();
            lol.savedComponentChips[i].outputPins = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(newValue));

            // Add to all 'inputPins' dictionary the property 'wireType' with a value
            // of 0 (at version 0.25 buses did not exist so its imposible for the wire
            // to be of other type)
            for (int j = 0; j < lol.savedComponentChips[i].inputPins.Count; j++)
            {
                newValue2.Add(new InputPin
                {
                    name = lol.savedComponentChips[i].inputPins[j].name,
                    parentChipIndex =
                      lol.savedComponentChips[i].inputPins[j].parentChipIndex,
                    parentChipOutputIndex =
                      lol.savedComponentChips[i].inputPins[j].parentChipOutputIndex,
                    isCylic = lol.savedComponentChips[i].inputPins[j].isCylic,
                    wireType = 0

                });
            }
            lol.savedComponentChips[i].inputPins =
                JsonConvert.DeserializeObject<dynamic>(
                    JsonConvert.SerializeObject(newValue2));
        }
        DirtyBit = true;
    }

    public static void From037to038(dynamic lol)
    {
        if (lol.Data == null)
        {
            //replace old sparse data chip with new datachip

            lol.Data = JObject.FromObject(new ChipData()
            {
                name = lol.name,
                creationIndex = lol.creationIndex,
                Colour = ((JObject)lol.colour).ToObject<Color>(),
                NameColour = ((JObject)lol.nameColour).ToObject<Color>(),
                folderName = lol.folderName,
                scale = lol.scale
            }, JsonSerializer.Create(GenerateConverterForColor()));
            lol.Property("name").Remove();
            lol.Property("creationIndex").Remove();
            lol.Property("colour").Remove();
            lol.Property("nameColour").Remove();
            lol.Property("folderName").Remove();
            lol.Property("scale").Remove();
        }

        if (lol.ChipDependecies == null && lol.componentNameList != null)
        {
            lol.ChipDependecies = lol.componentNameList;
            lol.Property("componentNameList").Remove();
        }

        DirtyBit = true;
    }


    private static JsonSerializerSettings GenerateConverterForColor()
    {
        var JsonConverteForColor = new JsonSerializerSettings();
        JsonConverteForColor.Converters.Add(new ColorConverter());
        return JsonConverteForColor;
    }
}
