using Partlyx.Core;

namespace Partlyx.Data
{
    public interface IResourceRepository
    {
        Task<Guid> AddAsync(Resource resource);
        Task DeleteAsync(Guid uid);
        Task<Guid> DuplicateAsync(Guid uid);
        Task ExecuteOnResourceAsync(Guid resourceUid, Func<Resource, Task> action);
        Task<Resource?> GetByUidAsync(Guid uid);
        Task<List<Resource>> SearchAsync(string query);

        // ExecuteOnPart methods
        Task<TResult> ExecuteOnComponentAsync<TResult>(Guid resourceUid, Guid componentUid, Func<RecipeComponent, Task<TResult>> action);
        Task ExecuteOnComponentAsync(Guid resourceUid, Guid componentUid, Func<RecipeComponent, Task> action);
        Task<TResult> ExecuteOnRecipeAsync<TResult>(Guid resourceUid, Guid recipeUid, Func<Recipe, Task<TResult>> action);
        Task ExecuteOnRecipeAsync(Guid resourceUid, Guid recipeUid, Func<Recipe, Task> action);
        Task<TResult> ExecuteOnResourceAsync<TResult>(Guid resourceUid, Func<Resource, Task<TResult>> action);
    }
}