using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceImplementations;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IRecipeService
    {
        Task<Guid> CreateRecipeAsync(RecipeCreatingOptions? opt = null);
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
