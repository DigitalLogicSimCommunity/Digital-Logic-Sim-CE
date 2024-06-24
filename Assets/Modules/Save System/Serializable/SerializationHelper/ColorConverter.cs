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
            return objectType == typeof(Color);
        }


        public static JsonSerializer GenerateSerializerConverter()
        {
            var JsonConverteForColor = new JsonSerializerSettings();
            JsonConverteForColor.Converters.Add(new ColorConverter());
            return JsonSerializer.Create(JsonConverteForColor);
        }

    }

    public class ColorConverterHEX : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color color)
            {
                // Convert the color to a hexadecimal string
                string hexColor = ColorUtility.ToHtmlStringRGBA(color);
                writer.WriteValue("#"+hexColor );
            }
            else
            {
                throw new JsonSerializationException("Expected Color object value.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
                // Read the hexadecimal color string
                string hexColor = (string)reader.Value;

                // Convert the hexadecimal string back to a Color object
                ColorUtility.TryParseHtmlString(hexColor, out Color color);
                return color;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public static JsonSerializerSettings GenerateSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorConverterHEX());
            return settings;
        }
        public static JsonSerializer GenerateSerializerConverter()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorConverterHEX());
            return JsonSerializer.Create(settings);
        }
    }
}