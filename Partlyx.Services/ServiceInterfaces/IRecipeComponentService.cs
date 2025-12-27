using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IRecipeComponentService
    {
        Task<Guid> CreateInputAsync(Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null);
        Task<Guid> CreateOutputAsync(Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null);
        Task DeleteComponentAsync(Guid componentUid);
        Task<Guid> DuplicateComponentAsync(Guid componentUid);
        Task<RecipeComponentDto?> GetComponentAsync(Guid componentUid);
        Task<bool> IsComponentExists(Guid componentUid);
        Task MoveComponentAsync(Guid parentRecipeUid, Guid newParentRecipeUid, Guid componentUid);
        Task SetComponentResourceAsync(Guid componentUid, Guid resourceToSelectUid);
        Task SetQuantityAsync(Guid componentUid, double quantity);
        Task SetResourceSelectedRecipeAsync(Guid componentUid, Guid? recipeToSelectUid);
        Task<List<RecipeComponentDto>> GetAllTheComponentsAsync(Guid parentRecipeUid);
    }
}
