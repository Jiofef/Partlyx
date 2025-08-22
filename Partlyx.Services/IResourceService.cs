using Partlyx.Services.Dtos;

namespace Partlyx.Services
{
    public interface IResourceService
    {
        Task<int> CreateResourceAsync();
        Task DeleteResourceAsync(int id);
        Task<ResourceDto?> GetResourceAsync(int id);
        Task<List<ResourceDto>> SearchResourcesAsync(string SearchQuery);
    }
}