using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeartPlayer.Utils
{
    internal class FileHelper
    {
        public static void SaveAsJson<T>(T t, string file)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(t, options);
            File.WriteAllText(file, json);
        }

        public static T ReadFromJson<T>(string file)
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
