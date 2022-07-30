using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FolderSystem
{
    private static Dictionary<int, string> Folders;

    public static void Init()
    {
        Folders = FolderLoader.DefaultFolderIndex;
        LoadFolders();
    }

    public static void Reset()
    {
        Folders = null;
    }
    public static void LoadFolders()
    {
        foreach (var kv in SaveSystem.LoadCustomFolders())
            Folders.TryAdd(kv.Key, kv.Value);
    }
    public static int ReverseIndex(string DicValue) => Folders.FirstOrDefault(x => x.Value == DicValue).Key;

    public static bool ContainsIndex(int i) => Folders.ContainsKey(i);

    public static void AddFolder(string newFolderName)
    {
        Folders[Folders.Count] = newFolderName;
        SaveSystem.SaveCustomFolders(Folders);
    }

    public static bool FolderNameAvailable(string name)
    {
        foreach (string f in Folders.Values)
        {
            if (string.Equals(name.ToUpper(), f.ToUpper()))
                return false;
        }
        return true;
    }



    public static IEnumerable<KeyValuePair<int, string>> Enum()
    {
        return Folders.AsEnumerable();
    }

    public static bool CompareValue(int index, string value) => string.Equals(Folders[index], value);

    public static void RenameFolder(string OldFolderName, string NewFolderName)
    {
        if (!Folders.ContainsValue(OldFolderName)
            || string.Equals(OldFolderName, NewFolderName))
            return;

        var index = ReverseIndex(OldFolderName);
        Folders[index] = NewFolderName;

        SaveSystem.SaveCustomFolders(Folders);
    }
}