using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Data
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly IDbContextFactory<PartlyxDBContext> _dbFactory;
        public ResourceRepository(IDbContextFactory<PartlyxDBContext> dbFactory) => _dbFactory = dbFactory;


        public async Task<Guid> AddAsync(Resource resource)
        {
            await using var db = _dbFactory.CreateDbContext();

            db.Resources.Add(resource);
            await db.SaveChangesAsync();
            return resource.Uid;
        }

        public async Task<Guid> DuplicateAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.Include(x => x.Recipes)
            .ThenInclude(rc => rc.Components)
            .FirstOrDefaultAsync(x => x.Uid == uid);

            if (r == null) throw new Exception("Cannot duplicate a non existing resource with Uid: " + uid);

            var duplicate = r.Clone();
            db.Resources.Add(duplicate);
            await db.SaveChangesAsync();
            return duplicate.Uid;
        }

        public async Task DeleteAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.FindAsync(uid);
            if (r != null)
            {
                db.Resources.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Important note. If you want to change the state of the received Resource or its descendants, use one of "ExecuteOn___Async" instead so that your changes are saved correctly.
        /// </summary>
        public async Task<Resource?> GetByUidAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.Include(x => x.Recipes)
                .ThenInclude(rc => rc.Components)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uid == uid);

            return r;
        }

        public async Task<List<Resource>> SearchAsync(string query)
        {
            await using var db = _dbFactory.CreateDbContext();

            var rl = await db.Resources.
                Where(r => EF.Functions.Like(r.Name, $"%{query}")).
                ToListAsync();

            return rl;
        }

        #region ExecuteOnPart methods
        InvalidOperationException ResourceNotFound(Guid resourceUid) 
            => new InvalidOperationException("Resource not found with Uid: " + resourceUid);

        public async Task<TResult> ExecuteOnResourceAsync<TResult>(Guid resourceUid, Func<Resource, Task<TResult>> action)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.Include(x => x.Recipes)
            .ThenInclude(rc => rc.Components)
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
                .FirstOrDefaultAsync(r => r.Uid == resourceUid);

            if (resource == null) throw ResourceNotFound(resourceUid);

            // Find a recipe that contains the component
            var recipe = resource.Recipes.FirstOrDefault(r => r.Uid == recipeUid);

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
                .FirstOrDefaultAsync(r => r.Uid == resourceUid);

            if (resource == null) throw ResourceNotFound(resourceUid);

            // Find a recipe that contains the component
            var recipe = resource.Recipes.FirstOrDefault(
                rp => rp.Components.Any(c => c.Uid == componentUid));

            if (recipe == null) throw new InvalidOperationException("Component's recipe not found in resource. Component's Uid: " + componentUid);

            var component = recipe.Components.First(c => c.Uid == componentUid);

            var result = await action(component);

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
        #endregion
    }
}
