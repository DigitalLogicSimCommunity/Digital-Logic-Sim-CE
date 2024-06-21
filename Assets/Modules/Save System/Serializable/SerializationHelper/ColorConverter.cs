using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DLS.SaveSystem.Serializable.SerializationHelper
{
    class ColorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = Regex.Unescape(JsonUtility.ToJson(value));
            writer.WriteRawValue(s);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            return jsonObject.ToObject<Color>(serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }


        public static JsonSerializer GenerateSerializerConverter()
        {
            var JsonConverteForColor = new JsonSerializerSettings();
            JsonConverteForColor.Converters.Add(new ColorConverter());
            return JsonSerializer.Create(JsonConverteForColor);
        }

        public static JsonSerializerSettings GenerateSettingsConverter()
        {
            var JsonConverteForColor = new JsonSerializerSettings();
            JsonConverteForColor.Converters.Add(new ColorConverter());
            return JsonConverteForColor;
        }
    }
}