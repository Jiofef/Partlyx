using Microsoft.EntityFrameworkCore;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public partial class PartlyxRepository : IPartlyxRepository
    {
        // Returns the found entities, as well as a list of not found uid
        public record BatchLoadResult
        {
            public IReadOnlyDictionary<Guid, Resource> Resources { get; init; } = new Dictionary<Guid, Resource>();
            public IReadOnlyDictionary<Guid, Recipe> Recipes { get; init; } = new Dictionary<Guid, Recipe>();
            public IReadOnlyDictionary<Guid, RecipeComponent> Components { get; init; } = new Dictionary<Guid, RecipeComponent>();
            public IReadOnlyCollection<Guid> MissingUids { get; init; } = Array.Empty<Guid>();
        }

        public class BatchIncludeOptions
        {
            public bool IncludeResourcesRecipes { get; set; } = false;       // Resource -> Recipes
            public bool IncludeRecipesComponents { get; set; } = false;      // Recipe -> Components
            public bool IncludeRecipeParentResource { get; set; } = false;   // Recipe -> ParentResource
            public bool IncludeComponentParentRecipe { get; set; } = false;  // Component -> ParentRecipe
            public bool IncludeComponentParentResource { get; set; } = false;// Component -> ParentRecipe -> ParentResource
            public bool IncludeComponentChildResource { get; set; } = false; // Component -> ChildResource
        }

        public async Task<BatchLoadResult> LoadBatchAsync(
            IEnumerable<Guid> resourceUids,
            IEnumerable<Guid> recipeUids,
            IEnumerable<Guid> componentUids,
            CancellationToken ct = default)
        {
            await using var db = _dbFactory.CreateDbContext();

            if (db == null)
                throw new InvalidOperationException();

            var resUids = (resourceUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();
            var recUids = (recipeUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();
            var compUids = (componentUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();

            var resources = resUids.Length > 0
                ? await db.Resources.Where(r => resUids.Contains(r.Uid)).ToListAsync(ct)
                : new List<Resource>();

            var recipes = recUids.Length > 0
                ? await db.Recipes.Where(r => recUids.Contains(r.Uid)).ToListAsync(ct)
                : new List<Recipe>();

            var components = compUids.Length > 0
                ? await db.RecipeComponents.Where(c => compUids.Contains(c.Uid)).ToListAsync(ct)
                : new List<RecipeComponent>();

            var foundUids = new HashSet<Guid>(
                resources.Select(r => r.Uid)
                .Concat(recipes.Select(r => r.Uid))
                .Concat(components.Select(c => c.Uid))
            );

            var requestedUids = resUids.Concat(recUids).Concat(compUids).ToArray();
            var missing = requestedUids.Where(u => !foundUids.Contains(u)).ToArray();

            return new BatchLoadResult
            {
                Resources = resources.ToDictionary(r => r.Uid),
                Recipes = recipes.ToDictionary(r => r.Uid),
                Components = components.ToDictionary(c => c.Uid),
                MissingUids = missing
            };
        }

        public async Task<TResult> ExecuteWithBatchAsync<TResult>(
            IEnumerable<Guid>? resourceUids,
            IEnumerable<Guid>? recipeUids,
            IEnumerable<Guid>? componentUids,
            BatchIncludeOptions? options,
            Func<BatchLoadResult, Task<TResult>> action,
            CancellationToken ct = default)
        {
            options ??= new BatchIncludeOptions();

            var resUids = (resourceUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();
            var recUids = (recipeUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();
            var compUids = (componentUids ?? Enumerable.Empty<Guid>()).Distinct().ToArray();

            await using var db = _dbFactory.CreateDbContext();

            // Resources query + optional includes
            IQueryable<Resource> resourcesQ = db.Resources;
            if (options.IncludeResourcesRecipes)
            {
                if (options.IncludeRecipesComponents)
                    resourcesQ = resourcesQ.Include(r => r.Recipes).ThenInclude(r => r.Components);
                else
                    resourcesQ = resourcesQ.Include(r => r.Recipes);
            }
            var resources = resUids.Length > 0 ? await resourcesQ.Where(r => resUids.Contains(r.Uid)).ToListAsync(ct) : new List<Resource>();

            // Recipes query + optional includes
            IQueryable<Recipe> recipesQ = db.Recipes;
            if (options.IncludeRecipesComponents)
                recipesQ = recipesQ.Include(r => r.Components);
            if (options.IncludeRecipeParentResource)
                recipesQ = recipesQ.Include(r => r.ParentResource);
            var recipes = recUids.Length > 0 ? await recipesQ.Where(r => recUids.Contains(r.Uid)).ToListAsync(ct) : new List<Recipe>();

            // Components query + optional includes
            IQueryable<RecipeComponent> compsQ = db.RecipeComponents;
            if (options.IncludeComponentParentRecipe)
                compsQ = compsQ.Include(c => c.ParentRecipe);
            if (options.IncludeComponentParentResource)
                compsQ = compsQ.Include(c => c.ParentRecipe).ThenInclude(r => r.ParentResource);
            if (options.IncludeComponentChildResource)
                compsQ = compsQ.Include(c => c.ComponentResource);
            var components = compUids.Length > 0 ? await compsQ.Where(c => compUids.Contains(c.Uid)).ToListAsync(ct) : new List<RecipeComponent>();

            // Build result
            var found = new HashSet<Guid>(
                resources.Select(r => r.Uid)
                .Concat(recipes.Select(r => r.Uid))
                .Concat(components.Select(c => c.Uid))
            );

            var requested = resUids.Concat(recUids).Concat(compUids).ToArray();
            var missing = requested.Where(u => !found.Contains(u)).ToArray();

            var batch = new BatchLoadResult
            {
                Resources = resources.ToDictionary(r => r.Uid),
                Recipes = recipes.ToDictionary(r => r.Uid),
                Components = components.ToDictionary(c => c.Uid),
                MissingUids = missing
            };

            // Execute user action in same DbContext
            var result = await action(batch);
            await db.SaveChangesAsync(ct);
            return result;
        }

        public async Task ExecuteWithBatchAsync(
            IEnumerable<Guid>? resourceUids,
            IEnumerable<Guid>? recipeUids,
            IEnumerable<Guid>? componentUids,
            BatchIncludeOptions? options,
            Func<BatchLoadResult, Task> action,
            CancellationToken ct = default)
        {
            await ExecuteWithBatchAsync<bool>(resourceUids, recipeUids, componentUids, options, async r =>
            {
                await action(r);
                return true;
            }, ct);
        }

    }
}
