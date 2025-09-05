using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;

namespace Partlyx.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IResourceRepository _repo;
        private readonly IEventBus _eventBus;
        public RecipeService(IResourceRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
        }

        public async Task<Guid> CreateRecipeAsync(Guid parentResourceUid)
        {
            var result = await _repo.ExecuteOnResourceAsync<Guid>(parentResourceUid, resource =>
            {
                var recipe = resource.CreateRecipe();
                return Task.FromResult(recipe.Uid);
            });

            var recipe = await GetRecipeAsync(parentResourceUid, result);
            if (recipe != null)
                _eventBus.Publish(new RecipeCreatedEvent(recipe));

            return result;
        }

        public async Task<Guid> DuplicateRecipeAsync(Guid parentResourceUid, Guid recipeUid)
        {
            var result = await _repo.ExecuteOnRecipeAsync<Guid>(parentResourceUid, recipeUid, recipe =>
            {
                var duplicate = recipe.CopyTo(recipe.ParentResource!);
                return Task.FromResult(duplicate.Uid);
            });

            var recipe = await GetRecipeAsync(parentResourceUid, result);
            if (recipe != null)
                _eventBus.Publish(new RecipeCreatedEvent(recipe));

            return result;
        }

        public async Task DeleteRecipeAsync(Guid parentResourceUid, Guid recipeUid)
        {
            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
            {
                recipe.Detach();
                return Task.CompletedTask;
            });

            _eventBus.Publish(new RecipeDeletedEvent(parentResourceUid, recipeUid));
        }

        public async Task QuantifyRecipeAsync(Guid parentResourceUid, Guid recipeUid)
        {
            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
            {
                recipe.MakeQuantified();
                return Task.CompletedTask;
            });
        }

        public async Task<RecipeDto?> GetRecipeAsync(Guid parentResourceUid, Guid recipeUid)
        {
            var resource = await _repo.GetByUidAsync(parentResourceUid);
            if (resource == null) return null;

            var recipe = resource.Recipes.FirstOrDefault(x => x.Uid == recipeUid);

            return recipe != null ? recipe.ToDto() : null;
        }

        public async Task<List<RecipeDto>> GetAllTheRecipesAsync(Guid parentResourceUid)
        {
            var resource = await _repo.GetByUidAsync(parentResourceUid);
            if (resource == null) 
                throw new InvalidOperationException("Resource not found with Uid: " + parentResourceUid);

            var recipes = resource.Recipes;

            var recipesDto = recipes.Select(r => r.ToDto()).ToList();

            return recipesDto;
        }

        public async Task SetRecipeCraftAmountAsync(Guid parentResourceUid, Guid recipeUid, double craftAmount)
        {
            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
            {
                recipe.CraftAmount = craftAmount;
                return Task.CompletedTask;
            });

            var recipe = await GetRecipeAsync(parentResourceUid, recipeUid);
            if (recipe != null)
                _eventBus.Publish(new RecipeUpdatedEvent(recipe, new[] { "CraftAmount" }));
        }
    }
}
