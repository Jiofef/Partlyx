using Partlyx.Infrastructure.Data.Interfaces;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class JsonSaver : IJsonSaver
    {
        public async Task<bool> TrySaveAsync(string path, string content)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(path))
                    await w.WriteAsync(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
