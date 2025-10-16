using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Services.ServiceImplementations
{
    public class RecipeComponentService : IRecipeComponentService
    {
        private readonly IPartsRepository _repo;
        private readonly IEventBus _eventBus;
        public RecipeComponentService(IPartsRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
        }

        public async Task<Guid> CreateComponentAsync(Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid)
        {
            var batchOptions = new PartsRepository.BatchIncludeOptions() { };
            var result = await _repo.ExecuteWithBatchAsync(
                [componentResourceUid], [parentRecipeUid], [], batchOptions,
                batch =>
            {
                var recipe = batch.Recipes[parentRecipeUid];
                var resource = batch.Resources[componentResourceUid];

                var component = recipe.CreateComponent(resource, 1);

                return Task.FromResult(component.Uid);
            });

            var component = await GetComponentAsync(grandParentResourceUid, result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component));

            return result;
        }

        public async Task<Guid> DuplicateComponentAsync(Guid grandParentResourceUid, Guid componentUid)
        {
            var result = await _repo.ExecuteOnComponentAsync(grandParentResourceUid, componentUid, component =>
            {
                var duplicate = component.CopyTo(component.ParentRecipe!);
                return Task.FromResult(duplicate.Uid);
            });

            var component = await GetComponentAsync(grandParentResourceUid, result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component));

            return result;
        }

        public async Task DeleteComponentAsync(Guid parentResourceUid, Guid componentUid)
        {
            Guid componentParentRecipeGuid = Guid.Empty;
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                componentParentRecipeGuid = component.ParentRecipe!.Uid;
                component.Detach();
                return Task.CompletedTask;
            });
            await _repo.DeleteComponentAsync(componentUid);

            _eventBus.Publish(new RecipeComponentDeletedEvent(parentResourceUid, componentParentRecipeGuid, componentUid));
        }

        public async Task MoveComponentAsync(Guid grandParentResourceUid, Guid newGrandParentResourceUid, Guid parentRecipeUid, Guid newParentRecipeUid, Guid componentUid)
        {
            RecipeComponent? component = null;
            await _repo.ExecuteOnComponentAsync(grandParentResourceUid, componentUid, async _component  =>
            {
                _component.Detach();
                component = _component;

                return Task.CompletedTask;
            });
            await _repo.ExecuteOnRecipeAsync(newGrandParentResourceUid, newParentRecipeUid, recipe =>
            {
                component?.AttachTo(recipe);
                return Task.CompletedTask;
            });

            _eventBus.Publish(new RecipeComponentMovedEvent(grandParentResourceUid, newGrandParentResourceUid, parentRecipeUid, newParentRecipeUid, componentUid));
        }

        public async Task<RecipeComponentDto?> GetComponentAsync(Guid parentResourceUid, Guid componentUid)
        {
            var resource = await _repo.GetByUidAsync(parentResourceUid);
            if (resource == null) return null;

            var component = resource.GetRecipeComponentByUid(componentUid);

            return component != null ? component.ToDto() : null;
        }

        public async Task<List<RecipeComponentDto>> GetAllTheComponentsAsync(Guid grandParentResourceUid, Guid parentRecipeUid)
        {
            var resource = await _repo.GetByUidAsync(grandParentResourceUid);
            if (resource == null) 
                throw new InvalidOperationException("Resource not found with Uid: " + grandParentResourceUid); ;

            var recipe = resource.GetRecipeByUid(parentRecipeUid);
            if (recipe == null) 
                throw new InvalidOperationException("Recipe not found with Uid: " + parentRecipeUid);

            var components = recipe.Components;
            var componentsDto = components.Select(c => c.ToDto()).ToList();

            return componentsDto;
        }

        public async Task SetQuantityAsync(Guid parentResourceUid, Guid componentUid, double quantity)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                component.Quantity = quantity;
                return Task.CompletedTask;
            });

            var component = await GetComponentAsync(parentResourceUid, componentUid);
            if (component != null)
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, new[] { "Quantity" }));
        }

        public async Task SetResourceSelectedRecipeAsync(Guid parentResourceUid, Guid componentUid, Guid? recipeToSelectUid)
        {
            var batchOptions = new PartsRepository.BatchIncludeOptions() { IncludeComponentChildResource = true, IncludeResourcesRecipes = true };

            if (recipeToSelectUid == null)
            {
                await _repo.ExecuteWithBatchAsync(
                [], [], [componentUid], batchOptions,
                batch =>
                {
                    var component = batch.Components[componentUid];

                    component.SetSelectedRecipe(null);
                    return Task.CompletedTask;
                });
            }
            else
            {
                await _repo.ExecuteWithBatchAsync(
                [], [(Guid)recipeToSelectUid!], [componentUid], batchOptions,
                batch =>
                {
                    var component = batch.Components[componentUid];
                    var recipeToSelect = batch.Recipes[(Guid)recipeToSelectUid];

                    component.SetSelectedRecipe(recipeToSelect);
                    return Task.CompletedTask;
                });
            }

            var component = await GetComponentAsync(parentResourceUid, componentUid);
            if (component != null)
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, new[] { "SelectedRecipeUid" }));
        }

        public async Task SetComponentResourceAsync(Guid parentResourceUid, Guid componentUid, Guid resourceToSelectUid)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, async component =>
            {
                var newComponentResource = await _repo.GetByUidAsync(resourceToSelectUid);

                if (newComponentResource == null)
                    throw new InvalidOperationException("Resource not found with Uid: " + resourceToSelectUid);

                component.SetComponentResource(newComponentResource);
            });

            var component = await GetComponentAsync(parentResourceUid, componentUid);
            if (component != null)
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, new[] { "ComponentResource" }));
        }
    }
}
