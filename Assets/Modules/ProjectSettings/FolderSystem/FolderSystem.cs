using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.ProjectSettings
{

    public partial class ProjectSettings
    {
        public static class FolderSystem
        {
            public enum DefaultKays
            {
                Comp = 0,
                Gate = 1,
                Misc = 2
            }

            private static bool Inizialized = false;

            public static IEnumerable<KeyValuePair<int, string>> Enum
            {
                get
                {
                    if (Folders is null || !Inizialized)
                        Init();
                    return Folders.AsEnumerable();
                }
            }

            public static Dictionary<int, string> DefaultFolder =>
                new Dictionary<int, string>()
                {
                    { (int)DefaultKays.Comp, "Comp" },
                    { (int)DefaultKays.Gate, "Gate" },
                    { (int)DefaultKays.Misc, "Misc" }
                };

            private static Dictionary<int, string> Folders;


            public static void Init()
            {
                Folders = new Dictionary<int, string>(DefaultFolder);

                foreach (var kv in SaveSystem.LoadProjectSettings().CustomFolders)
                    Folders.TryAdd(kv.Key, kv.Value);

                Inizialized = true;
            }

            public static void Reset()
            {
                Folders = null;
                Inizialized = false;
            }

            public static int ReverseIndex(string DicValue) =>
                Inizialized ? Folders.FirstOrDefault(x => x.Value == DicValue).Key : -1;

            public static bool ContainsIndex(int i) => Inizialized && Folders.ContainsKey(i);

            public static string GetFolderName(int i)
            {
                return !ContainsIndex(i) ? "" : Folders[i];
            }

            public static int AddFolder(string newFolderName)
            {
                if (!Inizialized) return -1;

                Folders[Folders.Count] = newFolderName;
                SaveSystem.SaveProjectSettings(Folders);
                return Folders.Count - 1;
            }

            public static void DeleteFolder(string folderName)
            {
                if (!Inizialized) return;

                DeleteFolder(ReverseIndex(folderName));
            }

            public static void DeleteFolder(int Index)
            {
                if (!Inizialized) return;
                Folders.Remove(Index);
                SaveSystem.SaveProjectSettings(Folders);
            }

            public static bool FolderNameAvailable(string name)
            {
                if (!Inizialized) return false;

                foreach (string f in Folders.Values)
                {
                    if (string.Equals(name.ToUpper(), f.ToUpper()))
                        return false;
                }

                return true;
            }


            public static bool CompareValue(int index, string value) =>
                Inizialized && ContainsIndex(index) && string.Equals(Folders[index], value);

            public static void RenameFolder(string OldFolderName, string NewFolderName)
            {
                if (!Inizialized) return;

                if (!Folders.ContainsValue(OldFolderName)
                    || string.Equals(OldFolderName, NewFolderName))
                    return;

                var index = ReverseIndex(OldFolderName);
                Folders[index] = NewFolderName;

                SaveSystem.SaveProjectSettings(Folders);
            }
        }
    }
}