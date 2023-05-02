using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveSystem
{

    static string activeProjectName = "Untitled";
    const string fileExtension = ".txt";
    static string CustomFoldersFileName = "CustomFolders";

    private static string FoldersFilePath => Path.Combine(CurrentSaveProfileDirectoryPath, CustomFoldersFileName + ".json");
    static string CurrentSaveProfileDirectoryPath => Path.Combine(SaveDataDirectoryPath, activeProjectName);

    public static string SaveDataDirectoryPath => Path.Combine(Application.persistentDataPath, "SaveData");
    static string CurrentSaveProfileWireLayoutDirectoryPath => Path.Combine(CurrentSaveProfileDirectoryPath, "WireLayout");
    static string EEPROMSaveFilePath => Path.Combine(CurrentSaveProfileDirectoryPath, "EEPROMContents.json");
    public static string GetPathToSaveFile(string saveFileName) => Path.Combine(CurrentSaveProfileDirectoryPath, saveFileName + fileExtension);

    public static string GetPathToWireSaveFile(string saveFileName) => Path.Combine(CurrentSaveProfileWireLayoutDirectoryPath, saveFileName + fileExtension);


    public static void SetActiveProject(string projectName)
    {
        activeProjectName = projectName;
    }

    public static void Init()
    {
        // Create save directory (if doesn't exist already)
        Directory.CreateDirectory(CurrentSaveProfileDirectoryPath);
        Directory.CreateDirectory(CurrentSaveProfileWireLayoutDirectoryPath);
        FolderLoader.CreateDefault(FoldersFilePath);
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

    public static void LoadAllChips(Manager manager)
    {
        // Load any saved chips
        ChipLoader.LoadAllChips(GetChipSavePaths(), manager);
    }

    public static SavedChip[] GetAllSavedChips()
    {
        // Load any saved chips
        return ChipLoader.GetAllSavedChips(GetChipSavePaths());
    }

    public static IDictionary<string, SavedChip> GetAllSavedChipsDic()
    {
        // Load any saved chips but is Dic
        return ChipLoader.GetAllSavedChipsDic(GetChipSavePaths());
    }

    public static void MigrateSaves()
	{
        //old appdata path is at ../../Sebastian Lague/Digital Logic Sim

        string oldAppDataPath = Path.Combine(new string[] { Directory.GetParent(Application.persistentDataPath).Parent.FullName, "Sebastian Lague", "Digital Logic Sim" });
        if (Directory.Exists(oldAppDataPath))
        {
            string oldSaveDataPath = Path.Combine(oldAppDataPath, "SaveData");
            string[] savedProjectPaths = Directory.GetDirectories(oldSaveDataPath);
            foreach(string path in savedProjectPaths)
			{
                string folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path));
                if (Directory.Exists(folderName)) folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path) + " - Copy");
                Directory.Move(path, folderName);
			}
            Directory.Delete(Path.Combine(Directory.GetParent(Application.persistentDataPath).Parent.FullName, "Sebastian Lague"), true);
        }
    }

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


    public static byte[] LoadEEPROMContents()
    {
        if (File.Exists(EEPROMSaveFilePath))
        {
            string jsonString = ReadFile(EEPROMSaveFilePath);
            return JsonConvert.DeserializeObject<byte[]>(
                jsonString);
        }
        return new byte[] { };
    }



    public static void SaveEEPROMContents(byte[] contents)
    {
        string jsonStr = JsonConvert.SerializeObject(contents, Formatting.Indented);
        WriteFile(EEPROMSaveFilePath, jsonStr);
    }


    public static Dictionary<int, string> LoadCustomFolders()
    {
        return FolderLoader.LoadCustomFolders(FoldersFilePath);
    }

    public static void SaveCustomFolders(Dictionary<int, string> folders)
    {
        FolderLoader.SaveCustomFolders(FoldersFilePath, folders);
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


    public static SavedChip ReadChip(string chipName) => JsonUtility.FromJson<SavedChip>(ReadFile(GetPathToSaveFile(chipName)));
    public static SavedWireLayout ReadWire(string wireFile) => JsonUtility.FromJson<SavedWireLayout>(ReadFile(GetPathToWireSaveFile(wireFile)));


    public static void WriteChip(string chipName, string saveString) => WriteFile(GetPathToSaveFile(chipName), saveString);
    public static void WriteWire(string chipName, string saveContent) => WriteFile(GetPathToWireSaveFile(chipName), saveContent);
    public static void WriteFoldersFile(string FolderFileStr) => WriteFile(FoldersFilePath, FolderFileStr);

}

