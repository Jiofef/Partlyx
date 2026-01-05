using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IPartlyxRepository
    {
        Task<Guid> AddResourceAsync(Resource resource);
        Task DeleteResourceAsync(Guid uid);
        Task<Guid> DuplicateResourceAsync(Guid uid);
        Task<Resource?> GetResourceByUidAsync(Guid uid);
        Task<Recipe?> GetRecipeByUidAsync(Guid uid);
        Task<RecipeComponent?> GetRecipeComponentByUidAsync(Guid uid);
        Task<List<Resource>> SearchResourcesAsync(string query);

        Task<Guid> AddRecipeAsync(Recipe recipe);
        Task<Guid> DuplicateRecipeAsync(Guid uid);

        // ExecuteOnPart methods
        Task<TResult> ExecuteOnResourceAsync<TResult>(Guid resourceUid, Func<Resource, Task<TResult>> action);
        Task ExecuteOnResourceAsync(Guid resourceUid, Func<Resource, Task> action);
        Task<TResult> ExecuteOnComponentAsync<TResult>(Guid componentUid, Func<RecipeComponent, Task<TResult>> action);
        Task ExecuteOnComponentAsync(Guid componentUid, Func<RecipeComponent, Task> action);
        Task<TResult> ExecuteOnRecipeAsync<TResult>(Guid recipeUid, Func<Recipe, Task<TResult>> action);
        Task ExecuteOnRecipeAsync(Guid recipeUid, Func<Recipe, Task> action);
        Task<List<Resource>> GetAllTheResourcesAsync();
        Task<List<Recipe>> GetAllTheRecipesAsync();
        Task<List<RecipeComponent>> GetAllTheRecipeComponentsAsync();
        Task ClearEverything();
        Task<PartlyxRepository.BatchLoadResult> LoadBatchAsync(IEnumerable<Guid> resourceUids, IEnumerable<Guid> recipeUids, IEnumerable<Guid> componentUids, CancellationToken ct = default);
        Task<TResult> ExecuteWithBatchAsync<TResult>(IEnumerable<Guid>? resourceUids, IEnumerable<Guid>? recipeUids, IEnumerable<Guid>? componentUids, PartlyxRepository.BatchIncludeOptions? options, Func<PartlyxRepository.BatchLoadResult, Task<TResult>> action, CancellationToken ct = default);
        Task DeleteRecipeAsync(Guid uid);
        Task DeleteComponentAsync(Guid uid);
        Task ExecuteWithBatchAsync(IEnumerable<Guid>? resourceUids, IEnumerable<Guid>? recipeUids, IEnumerable<Guid>? componentUids, PartlyxRepository.BatchIncludeOptions? options, Func<PartlyxRepository.BatchLoadResult, Task> action, CancellationToken ct = default);
        Task DeleteWorkingDBFile();
        Task<int> GetResourcesCountAsync();
        Task<string> GetUniqueResourceNameAsync(string baseName);
        Task<string> GetUniqueRecipeNameAsync(string baseName);
    }
}
