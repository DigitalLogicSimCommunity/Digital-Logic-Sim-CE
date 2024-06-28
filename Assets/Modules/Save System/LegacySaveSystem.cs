using System.IO;
using DLS.SaveSystem.Serializable.SerializationHelper;
using Newtonsoft.Json;
using UnityEngine;

public partial class SaveSystem
{
    public static class Legacy
    {
        public static string WireLayoutPath => Path.Combine(ActiveProjectPath, "WireLayout");

        public static string SerializeWireLayout(SavedWireLayout wireLayout) =>
            JsonConvert.SerializeObject(wireLayout, Formatting.Indented, ColorConverterHEX.GenerateSerializerSettings());

        public static SavedWireLayout DeserializeWireLayout(string wireLayoutString) =>
            JsonConvert.DeserializeObject<SavedWireLayout>(wireLayoutString);

        public static string GetPathToWireSaveFile(string chipName, string ExatraPath = "") =>
            Path.Combine(WireLayoutPath + ExatraPath, chipName + ".txt");



        public static void SaveWireLayout(string chipName, SavedWireLayout wireLayout, string ExatraPath = "") =>
            WriteFile(GetPathToWireSaveFile(chipName, ExatraPath), SerializeWireLayout(wireLayout));

        public static SavedWireLayout ReadWireLayout(string wireFile) =>
            DeserializeWireLayout(ReadFile(GetPathToWireSaveFile(wireFile)));
    }


}