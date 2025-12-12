using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IRecipeComponentService
    {
        Task<Guid> CreateComponentAsync(Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null);
        Task DeleteComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task<Guid> DuplicateComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task<RecipeComponentDto?> GetComponentAsync(Guid parentResourceUid, Guid componentUid);
        Task<bool> IsComponentExists(Guid parentResourceUid, Guid componentUid);
        Task MoveComponentAsync(Guid grandParentResourceUid, Guid newGrandParentResourceUid, Guid parentRecipeUid, Guid newParentRecipeUid, Guid componentUid);
        Task SetComponentResourceAsync(Guid parentResourceUid, Guid componentUid, Guid resourceToSelectUid);
        Task SetQuantityAsync(Guid parentResourceUid, Guid componentUid, double quantity);
        Task SetResourceSelectedRecipeAsync(Guid parentResourceUid, Guid componentUid, Guid? recipeToSelectUid);
    }
}