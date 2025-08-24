using Partlyx.Services.Dtos;

namespace Partlyx.Services
{
    public interface IRecipeService
    {
        Task<Guid> CreateRecipeAsync(Guid parentResourceUid);
        Task DeleteRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task<Guid> DuplicateRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task<RecipeDto?> GetRecipeAsync(Guid parentResourceUid, Guid recipeUid);
        Task SetRecipeCraftAmountAsync(Guid parentResourceUid, Guid recipeUid, double craftAmount);
    }
}