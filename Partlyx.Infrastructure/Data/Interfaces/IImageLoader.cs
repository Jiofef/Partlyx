namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IImageLoader
    {
        Task<byte[]?> TryLoadImageAsync(string path);
    }
}