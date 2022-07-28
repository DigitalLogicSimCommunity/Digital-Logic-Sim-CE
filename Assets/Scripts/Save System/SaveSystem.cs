using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveSystem
{

    static string activeProjectName = "Untitled";
    const string fileExtension = ".txt";

    public static void SetActiveProject(string projectName)
    {
        activeProjectName = projectName;
    }

    public static void Init()
    {
        // Create save directory (if doesn't exist already)
        Directory.CreateDirectory(CurrentSaveProfileDirectoryPath);
        Directory.CreateDirectory(CurrentSaveProfileWireLayoutDirectoryPath);
    }

    public static string[] GetChipSavePaths()
    {
        DirectoryInfo directory =
            new DirectoryInfo(CurrentSaveProfileDirectoryPath);
        FileInfo[] files = directory.GetFiles("*" + fileExtension);
        var filtered =
            files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        List<string> result = new List<string>(); foreach (var f in filtered)
        {
            result.Add(f.ToString());
        }
        return result.ToArray();
        // return Directory.GetFiles(CurrentSaveProfileDirectoryPath, "*" +
        // fileExtension);
    }

    public static void LoadAll(Manager manager)
    {
        // Load any saved chips
        ChipLoader.LoadAllChips(GetChipSavePaths(), manager);
    }

    public static SavedChip[] GetAllSavedChips()
    {
        // Load any saved chips
        return ChipLoader.GetAllSavedChips(GetChipSavePaths());
    }

    public static IDictionary<string,SavedChip> GetAllSavedChipsDic()
    {
        // Load any saved chips but is Dic
        return ChipLoader.GetAllSavedChipsDic(GetChipSavePaths());
    }

    public static string GetPathToSaveFile(string saveFileName)
    {
        return Path.Combine(CurrentSaveProfileDirectoryPath, saveFileName + fileExtension);
    }

    public static string GetPathToWireSaveFile(string saveFileName) => Path.Combine(CurrentSaveProfileWireLayoutDirectoryPath,saveFileName + fileExtension);

    static string CurrentSaveProfileDirectoryPath => Path.Combine(SaveDataDirectoryPath, activeProjectName);

    static string CurrentSaveProfileWireLayoutDirectoryPath => Path.Combine(CurrentSaveProfileDirectoryPath, "WireLayout");

    public static string[] GetSaveNames()
    {
        string[] savedProjectPaths = new string[0];
        if (Directory.Exists(SaveDataDirectoryPath))
        {
            savedProjectPaths = Directory.GetDirectories(SaveDataDirectoryPath);
        }
        for (int i = 0; i < savedProjectPaths.Length; i++)
        {
            string[] pathSections =
                savedProjectPaths[i].Split(Path.DirectorySeparatorChar);
            savedProjectPaths[i] = pathSections[pathSections.Length - 1];
        }
        return savedProjectPaths;
    }

    public static string SaveDataDirectoryPath
    {
        get
        {
            const string saveFolderName = "SaveData";
            return Path.Combine(Application.persistentDataPath, saveFolderName);
        }
    }

    public static Dictionary<string, List<int>> LoadHDDContents()
    {
        string hddSaveFile =
            Path.Combine(CurrentSaveProfileDirectoryPath, "HDDContents.json");
        if (File.Exists(hddSaveFile))
        {
            string jsonString = ReadFile(hddSaveFile);
            return JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(
                jsonString);
        }
        return new Dictionary<string, List<int>> { };
    }

    public static void SaveHDDContents(Dictionary<string, List<int>> contents)
    {
        string hddSaveFile =
            Path.Combine(CurrentSaveProfileDirectoryPath, "HDDContents.json");
        string jsonStr = JsonConvert.SerializeObject(contents);
        // Format json string
        jsonStr = jsonStr.Replace("{", "{\n\t")
                      .Replace("}", "\n}")
                      .Replace("],", "],\n\t");
        WriteFile(hddSaveFile, jsonStr);
    }

    public static Dictionary<string, int> LoadCustomFolders()
    {
        string customFoldersFile =
            Path.Combine(CurrentSaveProfileDirectoryPath, "CustomFolders.json");
        if (File.Exists(customFoldersFile))
        {
            string jsonString = ReadFile(customFoldersFile);
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);
            ;
        }
        return new Dictionary<string, int>();
    }

    public static void SaveCustomFolders(Dictionary<string, int> folders)
    {
        string customFoldersFile =
            Path.Combine(CurrentSaveProfileDirectoryPath, "CustomFolders.json");
        string jsonString =
            JsonConvert.SerializeObject(folders, Formatting.Indented);
        WriteFile(customFoldersFile, jsonString);
    }

    public static string ReadFile(string path)
    {
        using (StreamReader reader = new StreamReader(path))
        {
            return reader.ReadToEnd();
        }
    }

    public static void WriteFile(string path, string content)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(content);
        }
    }

    public static void RewriteChip(string chipName,string saveString)
    {
        WriteFile(GetPathToSaveFile(chipName), saveString);
    }

}
