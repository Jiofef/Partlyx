using Partlyx.Core;
using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IResourceService
    {
        Task<Guid> CreateResourceAsync();
        Task<Guid> DuplicateResourceAsync(Guid uid);
        Task DeleteResourceAsync(Guid uid);
        Task<ResourceDto?> GetResourceAsync(Guid uid);
        Task<List<ResourceDto>> SearchResourcesAsync(string SearchQuery);
        Task SetDefaultRecipeAsync(Guid resourceUid, Guid recipeUid);
        Task SetNameAsync(Guid resourceUid, string name);
        Task<List<ResourceDto>> GetAllTheResourcesAsync();
        Task<List<Guid>> SearchResourcesUidsAsync(string query);
    }
}