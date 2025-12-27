using Microsoft.EntityFrameworkCore;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public partial class PartlyxRepository : IPartlyxRepository
    {
        InvalidOperationException ResourceNotFound(Guid resourceUid)
             => new InvalidOperationException("Resource not found with Uid: " + resourceUid);

        public async Task<TResult> ExecuteOnResourceAsync<TResult>(Guid resourceUid, Func<Resource, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources
                .FirstOrDefaultAsync(x => x.Uid == resourceUid);

            if (r == null) throw ResourceNotFound(resourceUid);

            var result = await action(r);

            await db.SaveChangesAsync();

            return result;
        }


        public Task ExecuteOnResourceAsync(Guid resourceUid, Func<Resource, Task> action)
        {
            return ExecuteOnResourceAsync(resourceUid, async r =>
            {
                await action(r);
                return true; // Dummy value
            });
        }

        public async Task<TResult> ExecuteOnRecipeAsync<TResult>(Guid recipeUid,
                Func<Recipe, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var recipe = await db.Recipes
                .Include(rc => rc.Components)
                .ThenInclude(c => c.ComponentResource)
                .FirstOrDefaultAsync(r => r.Uid == recipeUid);

            if (recipe == null) throw new InvalidOperationException("Recipe not found with Uid: " + recipeUid);

            var result = await action(recipe);

            await db.SaveChangesAsync();
            return result;
        }

        public Task ExecuteOnRecipeAsync(Guid recipeUid,
                Func<Recipe, Task> action)
        {
            return ExecuteOnRecipeAsync(recipeUid, async r =>
            {
                await action(r);
                return true; // Dummy value
            });
        }

        public async Task<TResult> ExecuteOnComponentAsync<TResult>(Guid componentUid,
                Func<RecipeComponent, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var component = await db.RecipeComponents
                .Include(c => c.ComponentResource)
                .Include(c => c.ParentRecipe)
                .FirstOrDefaultAsync(c => c.Uid == componentUid);

            if (component == null) throw new InvalidOperationException("Component not found with Uid: " + componentUid);

            var result = await action(component);

            await db.SaveChangesAsync();
            return result;
        }

        public Task ExecuteOnComponentAsync(Guid componentUid,
                Func<RecipeComponent, Task> action)
        {
            return ExecuteOnComponentAsync(componentUid, async r =>
            {
                await action(r);
                return true; // Dummy value
            });
        }
    }
}
