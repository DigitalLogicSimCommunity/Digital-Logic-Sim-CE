using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using DLS.SaveSystem.Serializable.SerializationHelper;
using Modules.ProjectSettings;
using Modules.ProjectSettings.Serializable;
using UnityEngine;
using Newtonsoft.Json;

public static partial class SaveSystem
{
    public static string ActiveProjectName { get; set; } = "Untitled";
    public static string FileExtension { get; set; } = ".json";
    public static string ProjectSettingsFileName { get; private set; } = "CustomFolders";
    public static string ChipFolder { get; set; } = "Chips";

    private static string ProjectSettingsPath =>
        Path.Combine(ActiveProjectPath, ProjectSettingsFileName + FileExtension);

    static string ActiveProjectPath => Path.Combine(SaveDataDirectoryPath, ActiveProjectName);
    static string ChipPath => Path.Combine(ActiveProjectPath, ChipFolder);

    public static string SaveDataDirectoryPath => Path.Combine(Application.persistentDataPath, "SaveData");


    static string EEPROMSaveFilePath => Path.Combine(ActiveProjectPath, "EEPROMContents.json");

    public static string GetPathToChip(string chipName, string ExtraPath = "") =>
        Path.Combine(ChipPath + ExtraPath, chipName + FileExtension);


    public static void Init()
    {
        // Create save directory (if doesn't exist already)
        Directory.CreateDirectory(ActiveProjectPath);
        Directory.CreateDirectory(ChipPath);
        if (!File.Exists(ProjectSettingsPath))
            ProjectSettings.CreateDefault();
        UpdateProject();
    }

    public static FileInfo[] GetChipSavePaths()
    {
        DirectoryInfo directory = new DirectoryInfo(ChipPath);
        FileInfo[] files = directory.GetFiles("*" + FileExtension);
        var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        return filtered.ToArray();
    }

    public static void LoadAllChips(Manager manager)
    {
        ChipFolder = "Chips";
        FileExtension = ".json";
        // Load any saved chips
        ChipLoader.LoadAllChips(GetAllSavedChipsDic(), manager);
    }

    public static SavedChip[] GetAllSavedChips()
    {
        var chipPaths = GetChipSavePaths();
        var savedChips = new SavedChip[chipPaths.Length];

        // Read saved chips from file
        for (var i = 0; i < chipPaths.Length; i++)
        {
            var chipPath = chipPaths[i].FullName;
            var chipName = Path.GetFileNameWithoutExtension(chipPaths[i].Name);
            if (chipName.Equals(ProjectSettingsFileName)) continue;

            var chipSaveString = ReadFile(chipPath);
            savedChips[i] = DeserializeChip(chipSaveString);
        }

        foreach (var chip in savedChips)
            chip.ValidateDefaultData();

        return savedChips;
    }

    public static IDictionary<string, SavedChip> GetAllSavedChipsDic()
    {
        // Load any saved chips but is Dic
        return GetAllSavedChips()?.ToDictionary(chip => chip.Info.name);
    }


    public static byte[] LoadEEPROMContents()
    {
        if (!File.Exists(EEPROMSaveFilePath)) return new byte[] { };
        string jsonString = ReadFile(EEPROMSaveFilePath);
        return JsonConvert.DeserializeObject<byte[]>(
            jsonString);
    }


    public static void SaveEEPROMContents(byte[] contents)
    {
        string jsonStr = JsonConvert.SerializeObject(contents, Formatting.Indented);
        WriteFile(EEPROMSaveFilePath, jsonStr);
    }

    public static string ReadFile(string path)
    {
        using StreamReader reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    public static void WriteFile(string path, string content)
    {
        FileInfo FilePath = new FileInfo(path);
        Directory.CreateDirectory(FilePath.Directory.ToString());
        File.WriteAllText(path, content);
    }

    private static string SerializeProjectSettings(SavedProjectSettings settings) => JsonConvert.SerializeObject(settings, Formatting.Indented);
    private static SavedProjectSettings DeserializeProjectSettings(string settingsStr) => JsonConvert.DeserializeObject<SavedProjectSettings>(settingsStr);

    public static SavedProjectSettings LoadProjectSettings()
    {
        return ProjectSettings.LoadProjectSettings(ProjectSettingsPath);
    }

    public static void SaveProjectSettings(Dictionary<int, string> folders)
    {
        ProjectSettings.SaveProjectSettings(folders);
    }





    public static SavedChip DeserializeChip(string ChipSave) =>
        JsonConvert.DeserializeObject<SavedChip>(ChipSave, ColorConverterHEX.GenerateSerializerSettings());

    public static string SerializeChip(SavedChip chipSave) =>
        JsonConvert.SerializeObject(chipSave, Formatting.Indented, ColorConverterHEX.GenerateSerializerSettings());

    public static SavedChip ReadChip(string chipName) =>
        DeserializeChip((ReadFile(GetPathToChip(chipName))));

    public static void DeleteChip(string chipName) => File.Delete(GetPathToChip(chipName));


    public static void SaveChip(string chipName, SavedChip saveString, string ExatraPath = "") =>
        WriteFile(GetPathToChip(chipName, ExatraPath), SerializeChip(saveString));


    public static void WriteProjectSettings(string projectSettings) => WriteFile(ProjectSettingsPath, projectSettings);


    public static string[] GetProjectNames()
    {
        string[] savedProjectPaths = Array.Empty<string>();
        if (Directory.Exists(SaveDataDirectoryPath))
        {
            savedProjectPaths = Directory.GetDirectories(SaveDataDirectoryPath);
        }

        for (int i = 0; i < savedProjectPaths.Length; i++)
        {
            string[] pathSections =
                savedProjectPaths[i].Split(Path.DirectorySeparatorChar);
            savedProjectPaths[i] = pathSections[^1];
        }

        return savedProjectPaths;
    }

    public static void MigrateSaves()
    {
        //old appdata path is at ../../Sebastian Lague/Digital Logic Sim

        string oldAppDataPath = Path.Combine(new string[]
        {
            Directory.GetParent(Application.persistentDataPath).Parent.FullName, "Sebastian Lague", "Digital Logic Sim"
        });
        if (!Directory.Exists(oldAppDataPath)) return;

        string oldSaveDataPath = Path.Combine(oldAppDataPath, "SaveData");
        if (!Directory.Exists(oldSaveDataPath))
        {
            Debug.LogWarning("Failed migrating OldSave, the folder saveData was not found.");
            return;
        }

        string[] savedProjectPaths = Directory.GetDirectories(oldSaveDataPath);
        foreach (string path in savedProjectPaths)
        {
            string folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path));
            if (Directory.Exists(folderName))
                folderName = Path.Combine(SaveDataDirectoryPath, Path.GetFileName(path) + " - Copy");
            Directory.Move(path, folderName);
        }

        Directory.Delete(
            Path.Combine(Directory.GetParent(Application.persistentDataPath)?.Parent?.FullName ?? string.Empty,
                "Sebastian Lague"), true);
    }

    public static void ResetToDefaultSettings()
    {
        ChipFolder = "Chips";
        FileExtension = ".json";
    }


    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    public static void DeleteProject(string PorjectName)
    {
        ActiveProjectName = PorjectName;

        if (Directory.Exists(ActiveProjectPath))
           Directory.Delete(ActiveProjectPath, true);
        ActiveProjectName ="Untitled";
    }
    public static void MoveProject(string selectedProjectName, string newProjectName, bool keepOld = false)
    {
        ActiveProjectName = selectedProjectName;

        if (Directory.Exists(ActiveProjectPath))
        {
            var oldDirectory = ActiveProjectPath;
            ActiveProjectName = newProjectName;
            var newDirectory = ActiveProjectPath;
            if (keepOld)
                CopyFilesRecursively(oldDirectory, newDirectory);
            else
                Directory.Move(oldDirectory, newDirectory);

        }
        ActiveProjectName ="Untitled";

    }

    public static void UpdateProject()
    {
        FileExtension = ".txt";
        ChipFolder = "";
        FileInfo[] chipPaths = GetChipSavePaths();


        // Read saved chips from file
        foreach (var chipPat in chipPaths)
        {
            var chipPath = chipPat.FullName;
            var chipName = Path.GetFileNameWithoutExtension(chipPat.Name);
            if (chipName.Equals(ProjectSettingsFileName)) continue;

            var chipSaveString = ReadFile(chipPath);

            var updateSaveFile = SaveCompatibility.FixSaveCompatibility(chipSaveString, chipName);

            if (!SaveCompatibility.CanWriteFile) continue;
            SaveChip(chipName, updateSaveFile);
            File.Delete(chipPath);
            File.Delete(Legacy.GetPathToWireSaveFile(chipName));
        }

        if (Directory.Exists(Legacy.WireLayoutPath))
        {
            try
            {
                Directory.Delete(Legacy.WireLayoutPath);
            }
            catch (Exception e)
            {
                DLSLogger.LogWarning(e.Message);
            }

        }


        ResetToDefaultSettings();
    }
}