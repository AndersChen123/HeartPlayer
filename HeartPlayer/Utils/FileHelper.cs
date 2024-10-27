using Newtonsoft.Json;

namespace HeartPlayer.Utils
{
    internal class FileHelper
    {
        public static async Task SaveAsJson<T>(T t, string file)
        {
            var json = JsonConvert.SerializeObject(t);
            await File.WriteAllTextAsync(file, json);
        }

        public static async Task<T> ReadFromJson<T>(string file)
        {
            var json = await File.ReadAllTextAsync(file);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
