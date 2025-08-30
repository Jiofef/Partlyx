using Partlyx.Services.Dtos;

namespace Partlyx.Services
{
    public interface IRecipeComponentService
    {
        Task<Guid> CreateComponentAsync(Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid);
        Task DeleteComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task<Guid> DuplicateComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task<RecipeComponentDto?> GetComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task SetComponentResourceAsync(Guid parentResourceUid, Guid componentUid, Guid resourceToSelectUid);
        Task SetQuantityAsync(Guid parentResourceUid, Guid componentUid, double quantity);
        Task SetResourceSelectedRecipeAsync(Guid parentResourceUid, Guid componentUid, Guid resourceToSelectUid);
    }
}