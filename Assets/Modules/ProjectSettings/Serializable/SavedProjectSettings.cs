using System.Collections.Generic;

namespace Modules.ProjectSettings.Serializable
{
    [System.Serializable]
    public class SavedProjectSettings
    {
        public Dictionary<int, string> CustomFolders ;

        public SavedProjectSettings(Dictionary<int, string> customFolders = null)
        {
            CustomFolders = customFolders ?? new Dictionary<int, string>();
        }
    }
}