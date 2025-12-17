namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IJsonSaver
    {
        Task<bool> TrySaveAsync(string path, string content);
    }
}