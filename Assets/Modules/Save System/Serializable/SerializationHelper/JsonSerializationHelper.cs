using Newtonsoft.Json;

namespace DLS.SaveSystem.Serializable.SerializationHelper
{
    public class JsonSerializationHelper
    {
        public static JsonSerializerSettings GetSettingsColorAndSkip()
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new ColorConverter());
            jsonSerializerSettings.ContractResolver = ChipSerializerContract.Instance;
            jsonSerializerSettings.Formatting = Formatting.Indented;
            return jsonSerializerSettings;
        }
    }
}