using Partlyx.Core;
using Partlyx.Core.Partlyx;
using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IResourceService
    {
        Task<Guid> CreateResourceAsync(string? name = null);
        Task<Guid> DuplicateResourceAsync(Guid uid);
        /// <summary>
        /// Deletes a resource and all components that inherit it.
        /// Returns a list of deleted components for potential undo operations.
        /// </summary>
        Task<List<RecipeComponent>> DeleteResourceAsync(Guid uid);
        Task<ResourceDto?> GetResourceAsync(Guid uid);
        Task<List<ResourceDto>> SearchResourcesAsync(string SearchQuery);
        Task SetDefaultRecipeAsync(Guid resourceUid, Guid? recipeUid);
        Task SetNameAsync(Guid resourceUid, string name);
        Task<List<ResourceDto>> GetAllTheResourcesAsync();
        Task<List<Guid>> SearchResourcesUidsAsync(string query);
        Task<int> GetResourcesCountAsync();
        Task SetResourceIconAsync(Guid resourceUid, IconDto iconDto);
    }
}
