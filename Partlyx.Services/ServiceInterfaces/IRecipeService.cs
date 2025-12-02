using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IRecipeService
    {
        Task<Guid> CreateRecipeAsync(Guid parentResourceUid, string? recipeName = null);
        Task DeleteRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task<Guid> DuplicateRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task<List<RecipeDto>> GetAllTheRecipesAsync(Guid parentResourceUid);
        Task<RecipeDto?> GetRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task MoveRecipeAsync(Guid parentResourceUid, Guid newParentResourceUid, Guid recipeUid);
        Task QuantifyRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task SetRecipeIconAsync(Guid resourceUid, Guid recipeUid, IconDto iconDto);
        Task SetRecipeCraftAmountAsync(Guid parentResourceUid, Guid recipeUid, double craftAmount);
        Task SetRecipeNameAsync(Guid parentResourceUid, Guid recipeUid, string name);
        Task<bool> IsRecipeExists(Guid parentResourceUid, Guid recipeUid);
    }
}