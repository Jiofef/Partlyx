namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IJsonLoader
    {
        Task<T?> TryLoadAsAsync<T>(string path);
        Task<string?> TryLoadAsync(string path);
    }
}