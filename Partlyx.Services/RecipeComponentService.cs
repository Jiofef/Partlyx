using Partlyx.Core;
using Partlyx.Data;
using Partlyx.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services
{
    public class RecipeComponentService : IRecipeComponentService
    {
        private readonly ResourceRepository _repo;
        public RecipeComponentService(ResourceRepository repo) => _repo = repo;

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

            return result;
        }

        public async Task<Guid> DuplicateComponentAsync(Guid parentResourceUid, Guid componentUid)
        {
            var result = await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                var duplicate = component.CopyTo(component.ParentRecipe!);
                return Task.FromResult(duplicate.Uid);
            });

            return result;
        }

        public async Task DeleteComponentAsync(Guid parentResourceUid, Guid componentUid)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                component.Detach();
                return Task.CompletedTask;
            });
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

        public async Task SetQuantityAsync(Guid parentResourceUid, Guid componentUid, double quantity)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                component.Quantity = quantity;
                return Task.CompletedTask;
            });
        }

        public async Task SetResourceSelectedRecipeAsync(Guid parentResourceUid, Guid componentUid, Guid resourceToSelectUid)
        {
            await _repo.ExecuteOnComponentAsync(parentResourceUid, componentUid, component =>
            {
                var componentResource = component.ComponentResource;
                var resToSelect = componentResource.Recipes.FirstOrDefault(x => x.Uid == resourceToSelectUid);

                if (resToSelect == null)
                    throw new InvalidOperationException("Recipe not found with Uid: " + resourceToSelectUid);

                component.SetSelectedRecipe(resToSelect);
                return Task.CompletedTask;
            });
        }
    }
}
