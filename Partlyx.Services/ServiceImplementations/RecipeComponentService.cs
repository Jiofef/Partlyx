using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using System.ComponentModel;

namespace Partlyx.Services.ServiceImplementations
{
    public class RecipeComponentService : IRecipeComponentService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        public RecipeComponentService(IPartlyxRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
        }

        public async Task<Guid> CreateInputAsync(Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null)
        {
            return await CreateComponentAsync(parentRecipeUid, componentResourceUid, quantity, false);
        }

        public async Task<Guid> CreateOutputAsync(Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null)
        {
            return await CreateComponentAsync(parentRecipeUid, componentResourceUid, quantity, true);
        }

        private async Task<Guid> CreateComponentAsync(Guid parentRecipeUid, Guid componentResourceUid, double? quantity, bool isOutput)
        {
            var batchOptions = new PartlyxRepository.BatchIncludeOptions() { IncludeComponentChildResource = true };
            var result = await _repo.ExecuteWithBatchAsync(
                [componentResourceUid], [parentRecipeUid], [], batchOptions,
                batch =>
            {
                var recipe = batch.Recipes[parentRecipeUid];
                var resource = batch.Resources[componentResourceUid];

                double componentQuantity = quantity ?? 1;
                var component = isOutput ? recipe.CreateOutput(resource, componentQuantity) : recipe.CreateInput(resource, componentQuantity);

                return Task.FromResult(component.Uid);
            });

            var component = await GetComponentAsync(result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component, component.Uid));

            return result;
        }

        public async Task<Guid> CreateComponentAsync(Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid, double? quantity = null)
        {
            var batchOptions = new PartlyxRepository.BatchIncludeOptions() { };
            var result = await _repo.ExecuteWithBatchAsync(
                [componentResourceUid], [parentRecipeUid], [], batchOptions,
                batch =>
            {
                var recipe = batch.Recipes[parentRecipeUid];
                var resource = batch.Resources[componentResourceUid];

                double componentQuantity = quantity ?? 1;
                var component = recipe.CreateComponent(resource, componentQuantity);

                return Task.FromResult(component.Uid);
            });

            var component = await GetComponentAsync(result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component, component.Uid));

            return result;
        }

        public async Task<Guid> DuplicateComponentAsync(Guid grandParentResourceUid, Guid componentUid)
        {
            var result = await _repo.ExecuteOnComponentAsync(componentUid, component =>
            {
                var duplicate = component.CopyTo(component.ParentRecipe!);
                return Task.FromResult(duplicate.Uid);
            });

            var component = await GetComponentAsync(result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component, component.Uid));

            return result;
        }

        public async Task DeleteComponentAsync(Guid componentUid)
        {
            Guid componentParentRecipeGuid = Guid.Empty;
            await _repo.ExecuteOnComponentAsync(componentUid, component =>
            {
                componentParentRecipeGuid = component.ParentRecipe!.Uid;
                component.Detach();
                return Task.CompletedTask;
            });
            await _repo.DeleteComponentAsync(componentUid);

            _eventBus.Publish(new RecipeComponentDeletedEvent(componentParentRecipeGuid, componentUid,
                new HashSet<object>() { componentParentRecipeGuid, componentUid }));
        }

        public async Task<Guid> DuplicateComponentAsync(Guid componentUid)
        {
            var result = await _repo.ExecuteOnComponentAsync(componentUid, component =>
            {
                var duplicate = component.CopyTo(component.ParentRecipe!);
                return Task.FromResult(duplicate.Uid);
            });

            var component = await GetComponentAsync(result);
            if (component != null)
                _eventBus.Publish(new RecipeComponentCreatedEvent(component, component.ParentRecipeUid));

            return result;
        }

        public async Task MoveComponentAsync(Guid parentRecipeUid, Guid newParentRecipeUid, Guid componentUid)
        {
            RecipeComponent? component = null;
            await _repo.ExecuteOnComponentAsync(componentUid, async _component  =>
            {
                _component.Detach();
                component = _component;

                return Task.CompletedTask;
            });
            await _repo.ExecuteOnRecipeAsync(newParentRecipeUid, recipe =>
            {
                component?.AttachTo(recipe);
                return Task.CompletedTask;
            });

            _eventBus.Publish(new RecipeComponentMovedEvent(parentRecipeUid, newParentRecipeUid, componentUid,
                new HashSet<object>() { parentRecipeUid, newParentRecipeUid, componentUid}));
        }

        public async Task<RecipeComponentDto?> GetComponentAsync(Guid componentUid)
        {
            var component = await _repo.GetRecipeComponentByUidAsync(componentUid);

            return component != null ? component.ToDto() : null;
        }

        public async Task<bool> IsComponentExists(Guid componentUid)
            => await GetComponentAsync(componentUid) != null;

        public async Task<List<RecipeComponentDto>> GetAllTheComponentsAsync(Guid parentRecipeUid)
        {
            var recipe = await _repo.GetRecipeByUidAsync(parentRecipeUid);
            if (recipe == null) 
                throw new InvalidOperationException("Recipe not found with Uid: " + parentRecipeUid);

            var components = recipe.Components;
            var componentsDto = components.Select(c => c.ToDto()).ToList();

            return componentsDto;
        }

        public async Task SetQuantityAsync(Guid componentUid, double quantity)
        {
            double oldQuantity = 0;
            await _repo.ExecuteOnComponentAsync(componentUid, component =>
            {
                oldQuantity = component.Quantity;
                component.Quantity = quantity;
                return Task.CompletedTask;
            });

            var component = await GetComponentAsync(componentUid);
            if (component != null)
            {
                var changedProperties = new Dictionary<string, ChangedValuePair>
                {
                    { "Quantity", new ChangedValuePair(quantity, oldQuantity) }
                };
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, changedProperties, component.Uid));
            }
        }

        public async Task SetResourceSelectedRecipeAsync(Guid componentUid, Guid? recipeToSelectUid)
        {
            var batchOptions = new PartlyxRepository.BatchIncludeOptions() { IncludeComponentChildResource = true };

            Guid? oldRecipeUid = null;
            if (recipeToSelectUid == null)
            {
                await _repo.ExecuteWithBatchAsync(
                [], [], [componentUid], batchOptions,
                batch =>
                {
                    var component = batch.Components[componentUid];
                    oldRecipeUid = component.ComponentSelectedRecipeUid;

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
                    oldRecipeUid = component.ComponentSelectedRecipeUid;
                    var recipeToSelect = batch.Recipes[(Guid)recipeToSelectUid];

                    component.SetSelectedRecipe(recipeToSelect);
                    return Task.CompletedTask;
                });
            }

            var component = await GetComponentAsync(componentUid);
            if (component != null)
            {
                var changedProperties = new Dictionary<string, ChangedValuePair>
                {
                    { "SelectedRecipeUid", new ChangedValuePair(recipeToSelectUid ?? Guid.Empty, oldRecipeUid ?? Guid.Empty) }
                };
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, changedProperties, component.Uid));
            }
        }

        public async Task SetComponentResourceAsync(Guid componentUid, Guid resourceToSelectUid)
        {
            Guid oldResourceUid = Guid.Empty;
            await _repo.ExecuteOnComponentAsync(componentUid, async component =>
            {
                oldResourceUid = component.ComponentResource.Uid;

                var newComponentResource = await _repo.GetResourceByUidAsync(resourceToSelectUid);

                if (newComponentResource == null)
                    throw new InvalidOperationException("Resource not found with Uid: " + resourceToSelectUid);

                component.SetComponentResource(newComponentResource);
            });

            var component = await GetComponentAsync(componentUid);
            if (component != null)
            {
                var changedProperties = new Dictionary<string, ChangedValuePair>
                {
                    { "ResourceUid", new ChangedValuePair(resourceToSelectUid, oldResourceUid) }
                };
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, changedProperties, component.Uid));
            }
        }
    }
}
