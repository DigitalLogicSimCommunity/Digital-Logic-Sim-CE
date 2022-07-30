using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public static class FolderLoader
{
    private static Dictionary<int, string> DefaultFolder = new Dictionary<int, string>()
    {  
            { 0, "Comp" },
            { 1, "Gate" },
            { 2, "Misc"}
    };

    public static Dictionary<int, string> DefaultFolderIndex { get => new Dictionary<int, string>(DefaultFolder); }

    public static Dictionary<int, string> LoadCustomFolders(string path)
    {

        if (File.Exists(path))
        {
            string FoldersJson = SaveSystem.ReadFile(path);
            SaveCompatibility.FixFolderCompatibility(ref FoldersJson);
            return JsonConvert.DeserializeObject<Dictionary<int, string>>(FoldersJson);
        }
        return new Dictionary<int, string>();
    }

    public static void SaveCustomFolders(string path, Dictionary<int, string> folders)
    {
        string jsonString = JsonConvert.SerializeObject(folders, Formatting.Indented);
        SaveSystem.WriteFile(path, jsonString);
    }

    public static void CreateDefault(string path)
    {
        if (File.Exists(path)) return;

        SaveCustomFolders(path, DefaultFolder);

    }
}
