using Partlyx.Services.Dtos;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IRecipeService
    {
        Task<Guid> CreateRecipeAsync(string? recipeName = null, Guid? inheritedIconResourceUid = null);
        Task DeleteRecipeAsync(Guid recipeUid);
        Task<Guid> DuplicateRecipeAsync(Guid recipeUid);
        Task<List<RecipeDto>> GetAllTheRecipesAsync();
        Task<RecipeDto?> GetRecipeAsync(Guid recipeUid);
        Task QuantifyRecipeAsync(Guid recipeUid);
        Task SetRecipeIconAsync(Guid recipeUid, IconDto iconDto);
        Task SetRecipeIsReversibleAsync(Guid recipeUid, bool isReversible);
        Task SetRecipeNameAsync(Guid recipeUid, string name);
        Task<bool> IsRecipeExists(Guid recipeUid);
    }
}
