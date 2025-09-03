using Partlyx.Core;
using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services
{
    public class RecipeComponentService : IRecipeComponentService
    {
        private readonly IResourceRepository _repo;
        private readonly IEventBus _eventBus;
        public RecipeComponentService(IResourceRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
        }

        public async Task<Guid> CreateComponentAsync(Guid grandParentResourceUid, Guid parentRecipeUid, Guid componentResourceUid)
        {
            var result = await _repo.ExecuteOnRecipeAsync<Guid>(grandParentResourceUid, parentRecipeUid, async recipe =>
            {
                var componentResource = await _repo.GetByUidAsync(componentResourceUid);
                if (componentResource == null)
                    throw new InvalidOperationException("Component resource not found with Uid: " + componentResourceUid);

                var component = recipe.CreateComponent(componentResource, 1);
                return component.Uid;
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

            _eventBus.Publish(new RecipeComponentDeletedEvent(parentResourceUid, componentParentRecipeGuid, componentUid));
        }

        public async Task<RecipeComponentDto?> GetComponentAsync(Guid parentResourceUid, Guid componentUid)
        {
            var resource = await _repo.GetByUidAsync(parentResourceUid);
            if (resource == null) return null;

            // Find a recipe that contains the component
            var recipe = resource.Recipes.FirstOrDefault(
                rp => rp.Components.Any(c => c.Uid == componentUid));

            if (recipe == null) return null;

            var component = recipe.Components.First(c => c.Uid == componentUid);

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

        public async Task SetResourceSelectedRecipeAsync(Guid parentResourceUid, Guid componentUid, Guid recipeToSelectUid)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                var componentResource = component.ComponentResource;
                var recipeToSelect = componentResource.Recipes.FirstOrDefault(x => x.Uid == recipeToSelectUid);

                if (recipeToSelect == null)
                    throw new InvalidOperationException("Recipe not found with Uid: " + recipeToSelectUid);

                component.SetSelectedRecipe(recipeToSelect);
                return Task.CompletedTask;
            });

            var component = await GetComponentAsync(parentResourceUid, componentUid);
            if (component != null)
                _eventBus.Publish(new RecipeComponentUpdatedEvent(component, new[] { "ComponentSelectedRecipe" }));
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
