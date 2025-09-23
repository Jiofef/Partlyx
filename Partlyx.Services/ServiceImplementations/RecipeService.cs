using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using System.Diagnostics;

namespace Partlyx.Services.ServiceImplementations
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
            // If we change DefaultRecipe in ParentResource while performing the task from below, at the end of the method we will publish the event
            ResourceUpdatedEvent? defaultRecipeChangedEvent = null;

            var result = await _repo.ExecuteOnResourceAsync(parentResourceUid, resource =>
            {
                var recipe = resource.CreateRecipe();

                if (recipe.ParentResource?.DefaultRecipe == null)
                {
                    resource.SetDefaultRecipe(recipe);
                    defaultRecipeChangedEvent = new(resource.ToDto(), ["DefaultRecipeUid"]);
                }

                return Task.FromResult(recipe.Uid);
            });

            var recipe = await GetRecipeAsync(parentResourceUid, result);
            if (recipe != null)
            {
                _eventBus.Publish(new RecipeCreatedEvent(recipe));

                if (defaultRecipeChangedEvent != null)
                    _eventBus.Publish(defaultRecipeChangedEvent);
            }

            return result;
        }

        public async Task<Guid> DuplicateRecipeAsync(Guid parentResourceUid, Guid recipeUid)
        {
            var result = await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
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
            // If we change DefaultRecipe in ParentResource while performing the task from below, at the end of the method we will publish the event
            ResourceUpdatedEvent? defaultRecipeChangedEvent = null;

            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
            {
                var exParent = recipe.ParentResource!;
                recipe.Detach();

                if (exParent.DefaultRecipe == recipe)
                {
                    exParent.SetDefaultRecipe(null);
                    defaultRecipeChangedEvent = new(exParent.ToDto(), ["DefaultRecipeUid"]);
                }

                return Task.CompletedTask;
            });

            _eventBus.Publish(new RecipeDeletedEvent(parentResourceUid, recipeUid));
            if (defaultRecipeChangedEvent != null)
                _eventBus.Publish(defaultRecipeChangedEvent);
        }

        public async Task MoveRecipeAsync(Guid parentResourceUid, Guid newParentResourceUid, Guid recipeUid)
        {
            Recipe? recipe = null;

            // If we change DefaultRecipe in ParentResource while performing the task from below, at the end of the method we will publish the event
            ResourceUpdatedEvent? oldParentDefaultRecipeChangedEvent = null;
            ResourceUpdatedEvent? newParentDefaultRecipeChangedEvent = null;

            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, _recipe =>
            {
                var exParent = _recipe.ParentResource;
                _recipe.Detach();
                if (exParent!.DefaultRecipe == recipe)
                {
                    exParent.SetDefaultRecipe(null);
                    oldParentDefaultRecipeChangedEvent = new(exParent.ToDto(), ["DefaultRecipeUid"]);
                }

                recipe = _recipe;
                return Task.CompletedTask;
            });
            await _repo.ExecuteOnResourceAsync(newParentResourceUid, resource =>
            {
                recipe?.AttachTo(resource);

                if (resource.DefaultRecipe == recipe)
                {
                    resource.SetDefaultRecipe(null);
                    newParentDefaultRecipeChangedEvent = new(resource.ToDto(), ["DefaultRecipeUid"]);
                }

                return Task.CompletedTask;
            });

            _eventBus.Publish(new RecipeMovedEvent(parentResourceUid, newParentResourceUid, recipeUid));

            if (oldParentDefaultRecipeChangedEvent != null)
                _eventBus.Publish(oldParentDefaultRecipeChangedEvent);
            if (newParentDefaultRecipeChangedEvent != null)
                _eventBus.Publish(newParentDefaultRecipeChangedEvent);
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

            var recipe = resource.GetRecipeByUid(recipeUid);

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

        public async Task SetRecipeNameAsync(Guid parentResourceUid, Guid recipeUid, string name)
        {
            await _repo.ExecuteOnRecipeAsync(parentResourceUid, recipeUid, recipe =>
            {
                recipe.Name = name;
                return Task.CompletedTask;
            });

            var recipe = await GetRecipeAsync(parentResourceUid, recipeUid);
            if (recipe != null)
                _eventBus.Publish(new RecipeUpdatedEvent(recipe, new[] { "Name" }));
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
