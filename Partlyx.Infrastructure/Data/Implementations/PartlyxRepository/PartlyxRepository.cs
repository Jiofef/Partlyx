using Microsoft.EntityFrameworkCore;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public partial class PartlyxRepository : IPartlyxRepository
    {
        private readonly IDbContextFactory<PartlyxDBContext> _dbFactory;
        private readonly IEventBus _bus;
        public PartlyxRepository(IDbContextFactory<PartlyxDBContext> dbFactory, IEventBus bus)
        {
            _dbFactory = dbFactory;
            _bus = bus;
        }


        // Resource CRUD
        public async Task<Guid> AddResourceAsync(Resource resource)
        {
            await using var db = _dbFactory.CreateDbContext();

            db.Resources.Add(resource);
            await db.SaveChangesAsync();
            return resource.Uid;
        }

        public async Task<Guid> DuplicateResourceAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources
            .FirstOrDefaultAsync(x => x.Uid == uid);

            if (r == null) throw new Exception("Cannot duplicate a non existing resource with Uid: " + uid);

            var duplicate = r.Clone();
            db.Resources.Add(duplicate);
            await db.SaveChangesAsync();
            return duplicate.Uid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources
                .FirstOrDefaultAsync(r => r.Uid == uid);

            if (r != null)
            {
                db.Resources.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        public async Task<int> GetResourcesCountAsync()
        {
            await using var db = _dbFactory.CreateDbContext();

            var count = db.Resources.Count();
            return count;
        }

        // ---
        // Recipe CRUD

        public async Task<Guid> AddRecipeAsync(Recipe recipe)
        {
            await using var db = _dbFactory.CreateDbContext();

            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            return recipe.Uid;
        }

        public async Task<Guid> DuplicateRecipeAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Recipes.Include(rc => rc.Inputs).ThenInclude(c => c.ComponentResource)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            if (r == null) throw new Exception("Cannot duplicate a non existing recipe with Uid: " + uid);

            var duplicate = r.Clone();
            db.Recipes.Add(duplicate);
            await db.SaveChangesAsync();
            return duplicate.Uid;
        }

        public async Task DeleteRecipeAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Recipes
                .Include(rc => rc.Inputs)
                .FirstOrDefaultAsync(r => r.Uid == uid);

            if (r != null)
            {
                db.Recipes.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        // ---
        // Component CRUD

        public async Task DeleteComponentAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.RecipeComponents
                .FirstOrDefaultAsync(r => r.Uid == uid);

            if (r != null)
            {
                db.RecipeComponents.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Important note. If you want to change the state of the received Resource or its descendants, use one of "ExecuteOn___Async" instead so that your changes are saved correctly.
        /// </summary>
        public async Task<Resource?> GetResourceByUidAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uid == uid);

            return r;
        }

        public async Task<Recipe?> GetRecipeByUidAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.Recipes
                .Include(rc => rc.Inputs)
                .ThenInclude(c => c.ComponentResource)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uid == uid);

            return r;
        }

        public async Task<RecipeComponent?> GetRecipeComponentByUidAsync(Guid uid)
        {
            await using var db = _dbFactory.CreateDbContext();

            var r = await db.RecipeComponents
                .Include(c => c.ComponentResource)
                .Include(c => c.ParentRecipe)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Uid == uid);

            return r;
        }

        public async Task<List<Resource>> SearchResourcesAsync(string query)
        {
            await using var db = _dbFactory.CreateDbContext();

            var rl = await db.Resources.
                Where(r => EF.Functions.Like(r.Name, $"%{query}"))
                .AsNoTracking()
                .ToListAsync();

            return rl;
        }

        public async Task<List<Resource>> GetAllTheResourcesAsync()
        {
            await using var db = _dbFactory.CreateDbContext();

            var rl = await db.Resources
                .AsNoTracking()
                .ToListAsync();

            return rl;
        }

        public async Task<List<Recipe>> GetAllTheRecipesAsync()
        {
            await using var db = _dbFactory.CreateDbContext();

            var rl = await db.Recipes
                .Include(rc => rc.Inputs)
                .ThenInclude(c => c.ComponentResource)
                .AsNoTracking()
                .ToListAsync();

            return rl;
        }

        public async Task<List<RecipeComponent>> GetAllTheRecipeComponentsAsync()
        {
            await using var db = _dbFactory.CreateDbContext();

            var rl = await db.RecipeComponents
                .Include(c => c.ComponentResource)
                .Include(c => c.ParentRecipe)
                .AsNoTracking()
                .ToListAsync();

            return rl;
        }

        /// <summary>!!!</summary>
        public async Task ClearEverything()
        {
            await using var db = _dbFactory.CreateDbContext();

            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            _bus.Publish(new FileClearedEvent());
            _bus.Publish(new FileClosedEvent());
        }

        /// <summary>!!!!!</summary>
        public async Task DeleteWorkingDBFile()
        {
            await using var db = _dbFactory.CreateDbContext();

            // Closing connection
            await db.Database.EnsureDeletedAsync();
        }
    }
}
