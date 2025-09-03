using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class SelectedParts : ObservableObject, ISelectedParts
    {
        private ResourceItemViewModel? _resource;
        private RecipeItemViewModel? _recipe;
        private RecipeComponentItemViewModel? _component;

        public ResourceItemViewModel? Resource { get => _resource; private set => SetProperty(ref _resource, value); }

        public RecipeItemViewModel? Recipe { get => _recipe; private set => SetProperty(ref _recipe, value); }

        public RecipeComponentItemViewModel? Component { get => _component; private set => SetProperty(ref _component, value); }

        public void SetResource(ResourceItemViewModel? resource)
        {
            if (resource == Resource) return;

            Resource = resource;

            if (resource == null || resource.Recipes.Contains(Recipe))
            {
                Recipe = null;
                Component = null;
                return;
            }
        }

        public void SetRecipe(RecipeItemViewModel? recipe)
        {
            if (recipe == Recipe) return;

            Recipe = recipe;

            if (recipe == null || recipe.Components.Contains(_component))
            {
                Component = null;
                return;
            }
        }

        public void SetComponent(RecipeComponentItemViewModel? component)
        {
            if (component == Component) return;

            Component = component;
        }
    }
}
