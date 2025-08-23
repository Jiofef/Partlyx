using Partlyx.Services.Dtos;

namespace Partlyx.Services
{
    public interface IResourceService
    {
        Task<Guid> CreateResourceAsync();
        Task DeleteResourceAsync(Guid uid);
        Task<ResourceDto?> GetResourceAsync(Guid uid);
        Task<List<ResourceDto>> SearchResourcesAsync(string SearchQuery);
    }
}