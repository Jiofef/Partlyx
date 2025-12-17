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
                .Include(x => x.Recipes)
                .ThenInclude(rc => rc.Components)
                .ThenInclude(c => c.ComponentResource)
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

        public async Task<TResult> ExecuteOnRecipeAsync<TResult>(Guid resourceUid, Guid recipeUid,
                Func<Recipe, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var resource = await db.Resources
                .Include(r => r.Recipes)
                .ThenInclude(rc => rc.Components)
                .ThenInclude(c => c.ComponentResource)
                .FirstOrDefaultAsync(r => r.Uid == resourceUid);

            if (resource == null) throw ResourceNotFound(resourceUid);

            var recipe = resource.GetRecipeByUid(recipeUid);

            if (recipe == null) throw new InvalidOperationException("Recipe not found with Uid: " + recipeUid);

            var result = await action(recipe);

            await db.SaveChangesAsync();
            return result;
        }

        public Task ExecuteOnRecipeAsync(Guid resourceUid, Guid recipeUid,
                Func<Recipe, Task> action)
        {
            return ExecuteOnRecipeAsync(resourceUid, recipeUid, async r =>
            {
                await action(r);
                return true; // Dummy value
            });
        }

        public async Task<TResult> ExecuteOnComponentAsync<TResult>(Guid resourceUid, Guid componentUid,
                Func<RecipeComponent, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var resource = await db.Resources
                .Include(r => r.Recipes)
                .ThenInclude(rc => rc.Components)
                .ThenInclude(c => c.ComponentResource)
                .FirstOrDefaultAsync(r => r.Uid == resourceUid);

            if (resource == null) throw ResourceNotFound(resourceUid);

            // Find a recipe that contains the component
            var resourceComponent = resource.GetRecipeComponentByUid(componentUid);

            if (resourceComponent == null) throw new InvalidOperationException("Component was not found in resource. Component's Uid: " + componentUid);

            var result = await action(resourceComponent);

            await db.SaveChangesAsync();
            return result;
        }

        public Task ExecuteOnComponentAsync(Guid resourceUid, Guid componentUid,
                Func<RecipeComponent, Task> action)
        {
            return ExecuteOnComponentAsync(resourceUid, componentUid, async r =>
            {
                await action(r);
                return true; // Dummy value
            });
        }
    }
}
