using System.Collections.Generic;
using System.Diagnostics;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class VMPartsStore : IVMPartsStore
    {
        public Dictionary<Guid, ResourceItemViewModel> Resources { get; }
        public Dictionary<Guid, RecipeItemViewModel> Recipes { get; }
        public Dictionary<Guid, RecipeComponentItemViewModel> RecipeComponents { get; }

        public VMPartsStore()
        {
            Resources = new();
            Recipes = new();
            RecipeComponents = new();
        }

        public void Register(ResourceItemViewModel resource)
        {
            Resources.Add(resource.Uid, resource);
        }

        public void Register(RecipeItemViewModel recipe)
        {
            Recipes.Add(recipe.Uid, recipe);
        }

        public void Register(RecipeComponentItemViewModel component)
        {
            RecipeComponents.Add(component.Uid, component);
        }
    }
}
