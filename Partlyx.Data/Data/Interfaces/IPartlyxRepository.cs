using Partlyx.Core;
using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IPartlyxRepository
    {
        Task<Guid> AddResourceAsync(Resource resource);
        Task DeleteResourceAsync(Guid uid);
        Task<Guid> DuplicateResourceAsync(Guid uid);
        Task ExecuteOnResourceAsync(Guid resourceUid, Func<Resource, Task> action);
        Task<Resource?> GetResourceByUidAsync(Guid uid);
        Task<List<Resource>> SearchResourcesAsync(string query);

        // ExecuteOnPart methods
        Task<TResult> ExecuteOnComponentAsync<TResult>(Guid resourceUid, Guid componentUid, Func<RecipeComponent, Task<TResult>> action);
        Task ExecuteOnComponentAsync(Guid resourceUid, Guid componentUid, Func<RecipeComponent, Task> action);
        Task<TResult> ExecuteOnRecipeAsync<TResult>(Guid resourceUid, Guid recipeUid, Func<Recipe, Task<TResult>> action);
        Task ExecuteOnRecipeAsync(Guid resourceUid, Guid recipeUid, Func<Recipe, Task> action);
        Task<TResult> ExecuteOnResourceAsync<TResult>(Guid resourceUid, Func<Resource, Task<TResult>> action);
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
    }
}