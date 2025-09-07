namespace Partlyx.Services.ServiceInterfaces
{
    public interface IResourceImageIconService
    {
        Task SetImagePathAsync(Guid parentResourceUid, string path);
    }
}