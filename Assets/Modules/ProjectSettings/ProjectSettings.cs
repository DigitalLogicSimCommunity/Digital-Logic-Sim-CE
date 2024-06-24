using System.Collections.Generic;
using System.IO;
using Modules.ProjectSettings.Serializable;
using Newtonsoft.Json;

namespace Modules.ProjectSettings
{
    public static partial class ProjectSettings
    {
        private static SavedProjectSettings Settings;

        public static void Init()
        {
            Settings = SaveSystem.LoadProjectSettings();
        }

        public static SavedProjectSettings LoadProjectSettings(string path)
        {
            if (!File.Exists(path)) return new SavedProjectSettings();
            string FoldersJson = SaveSystem.ReadFile(path);
            SaveCompatibility.FixFolderCompatibility(ref FoldersJson);
            var folder =JsonConvert.DeserializeObject<Dictionary<int, string>>(FoldersJson);
            return new SavedProjectSettings(folder);
        }


        public static void SaveProjectSettings(IDictionary<int, string> folders)
        {
            string jsonString = JsonConvert.SerializeObject(folders, Formatting.Indented);
            SaveSystem.WriteProjectSettings(jsonString);
        }


        public static void CreateDefault()
        {
            SaveProjectSettings(FolderSystem.DefaultFolder);
        }
    }
}