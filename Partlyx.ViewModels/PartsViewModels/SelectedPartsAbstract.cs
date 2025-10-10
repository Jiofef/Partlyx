using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Partlyx.ViewModels.PartsViewModels
{
    public abstract partial class SelectedPartsAbstract : ObservableObject, ISelectedParts
    {
        [ObservableProperty]
        private bool _isSingleResourceSelected;
        [ObservableProperty]
        private bool _isSingleRecipeSelected;
        [ObservableProperty]
        private bool _isSingleComponentSelected;

        [ObservableProperty]
        private bool _isResourcesSelected;
        [ObservableProperty]
        private bool _isRecipesSelected;
        [ObservableProperty]
        private bool _isComponentsSelected;

        [ObservableProperty]
        private ResourceItemViewModel? _singleResourceOrNull;
        [ObservableProperty]
        private RecipeItemViewModel? _singleRecipeOrNull;
        [ObservableProperty]
        private RecipeComponentItemViewModel? _singleComponentOrNull;

        public ObservableCollection<ResourceItemViewModel> Resources { get; }

        public ObservableCollection<RecipeItemViewModel> Recipes { get; }

        public ObservableCollection<RecipeComponentItemViewModel> Components { get; }

        public SelectedPartsAbstract()
        {
            Resources = new();
            Recipes = new();
            Components = new();

            Resources.CollectionChanged += (sender, evInfo) => 
            {
                IsSingleResourceSelected = Resources.Count == 1;
                SingleResourceOrNull = GetSingleResourceOrNull();

                IsResourcesSelected = Resources.Count > 0;
                SelectedResourcesChangedHandler(sender, evInfo);
            };
            Recipes.CollectionChanged += (sender, evInfo) =>
            {
                IsSingleRecipeSelected = Recipes.Count == 1;
                SingleRecipeOrNull = GetSingleRecipeOrNull();

                IsRecipesSelected = Recipes.Count > 0;
                SelectedRecipesChangedHandler(sender, evInfo);
            };
            Components.CollectionChanged += (sender, evInfo) =>
            {
                IsSingleComponentSelected = Components.Count == 1;
                SingleComponentOrNull = GetSingleComponentOrNull();

                IsComponentsSelected = Components.Count > 0;
                SelectedComponentsChangedHandler(sender, evInfo);
            };
        }

        protected virtual void SelectedResourcesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedRecipesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedComponentsChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }

        #region Resource methods
        public void SelectSingleResource(ResourceItemViewModel resource)
        {
            if (Resources.Count == 1 && Resources.Contains(resource)) return;

            ClearSelectedResources();
            
            Resources.Add(resource);
        }

        public void AddResourceToSelected(ResourceItemViewModel resource)
        {
            if (Resources.Contains(resource!)) return;
            Resources.Add(resource);
        }

        public void ClearSelectedResources()
        {
            Resources.Clear();
        }

        public ResourceItemViewModel? GetSingleResourceOrNull()
        {
            bool isResourceSingle = Resources.Count == 1;
            return isResourceSingle ? Resources.Single() : null;
        }
        #endregion

        #region Recipe methods
        public void SelectSingleRecipe(RecipeItemViewModel recipe)
        {
            if (Recipes.Count == 1 && Recipes.Contains(recipe)) return;

            ClearSelectedRecipes();

            Recipes.Add(recipe);
        }

        public void SelectSingleRecipeAncestor(RecipeItemViewModel recipe)
        {
            if (recipe.LinkedParentResource?.Value is ResourceItemViewModel parentResource)
                SelectSingleResource(parentResource);
            else
                ClearSelectedResources();
        }

        public void AddRecipeToSelected(RecipeItemViewModel recipe)
        {
            if (Recipes.Contains(recipe!)) return;
            Recipes.Add(recipe);
        }

        public void ClearSelectedRecipes()
        {
            Recipes.Clear();
        }

        public RecipeItemViewModel? GetSingleRecipeOrNull()
        {
            bool isRecipeSingle = Recipes.Count == 1;
            return isRecipeSingle ? Recipes.Single() : null;
        }
        #endregion

        #region Component methods
        public void SelectSingleComponent(RecipeComponentItemViewModel component)
        {
            if (Components.Count == 1 && Components.Contains(component)) return;

            ClearSelectedComponents();

            Components.Add(component);
        }

        public void SelectSingleComponentAncestors(RecipeComponentItemViewModel component)
        {
            if (component.LinkedParentRecipe?.Value is RecipeItemViewModel parentRecipe)
            {
                SelectSingleRecipe(parentRecipe);
                SelectSingleRecipeAncestor(parentRecipe);
            }
            else
            {
                ClearSelectedRecipes();
                ClearSelectedResources();
            }
        }

        public void AddComponentToSelected(RecipeComponentItemViewModel component)
        {
            if (Components.Contains(component!)) return;
            Components.Add(component);
        }

        public void ClearSelectedComponents()
        {
            Components.Clear();
        }

        public RecipeComponentItemViewModel? GetSingleComponentOrNull()
        {
            bool isComponentSingle = Components.Count == 1;
            return isComponentSingle ? Components.Single() : null;
        }
        #endregion

        #region Generic / any parts
        public void SelectSinglePart(IVMPart part)
        {
            if (part is ResourceItemViewModel resource)
            {
                SelectSingleResource(resource);
            }
            else if (part is RecipeItemViewModel recipe)
            {
                SelectSingleRecipe(recipe);
            }
            else if (part is RecipeComponentItemViewModel component)
            {
                SelectSingleComponent(component);
            }
        }
        public void ClearSelection()
        {
            ClearSelectedResources();
            ClearSelectedRecipes();
            ClearSelectedComponents();
        }
        #endregion
    }
}
