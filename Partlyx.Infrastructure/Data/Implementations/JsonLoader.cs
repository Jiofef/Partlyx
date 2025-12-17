using Partlyx.Infrastructure.Data.Interfaces;
using System.Text.Json;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class JsonLoader : IJsonLoader
    {
        public async Task<string?> TryLoadAsync(string path)
        {
            try
            {
                using StreamReader r = File.OpenText(path);
                string json = await r.ReadToEndAsync();
                return json;
            }
            catch
            {
                return null;
            }
        }

        public async Task<T?> TryLoadAsAsync<T>(string path)
        {
            var json = await TryLoadAsync(path);
            if (json == null)
                return default;
            var deserialized = JsonSerializer.Deserialize<T>(json);
            return deserialized;
        }
    }
}
