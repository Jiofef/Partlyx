using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IPartlyxImageService
    {
        Task DeleteImageAsync(Guid uid);
        Task<byte[]?> GetFullImageOrNullAsync(Guid uid);
        Task<ImageDto?> GetImageOrNullAsync(Guid uid);
        bool IsFileSupported(string path);
        Task SetImageNameAsync(Guid uid, string name);
        Task<byte[]?> TryLoadFromDiskAsync(string path);
        Task<Guid?> TryLoadFromDiskToDbAsync(string path);
    }
}