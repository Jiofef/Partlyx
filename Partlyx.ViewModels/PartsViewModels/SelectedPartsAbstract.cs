using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Partlyx.ViewModels.PartsViewModels
{
    public abstract class SelectedPartsAbstract : ObservableObject, ISelectedParts
    {
        private bool _isSingleResourceSelected;
        private bool _isSingleRecipeSelected;
        private bool _isSingleComponentSelected;

        public ObservableCollection<ResourceItemViewModel> Resources { get; }

        public ObservableCollection<RecipeItemViewModel> Recipes { get; }

        public ObservableCollection<RecipeComponentItemViewModel> Components { get; }

        public SelectedPartsAbstract()
        {
            Resources = new();
            Recipes = new();
            Components = new();

            Resources.CollectionChanged += (obj, evInfo) => 
            {
                IsSingleResourceSelected = Resources.Count == 1;
                SelectedResourcesChangedHandler(obj, evInfo);
            };
            Recipes.CollectionChanged += (obj, evInfo) =>
            {
                IsSingleResourceSelected = Recipes.Count == 1;
                SelectedRecipesChangedHandler(obj, evInfo);
            };
            Components.CollectionChanged += (obj, evInfo) =>
            {
                IsSingleResourceSelected = Components.Count == 1;
                SelectedComponentsChangedHandler(obj, evInfo);
            };
        }

        protected virtual void SelectedResourcesChangedHandler(object? @object, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedRecipesChangedHandler(object? @object, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedComponentsChangedHandler(object? @object, NotifyCollectionChangedEventArgs args) { }

        public bool IsSingleResourceSelected 
        {
            get => _isSingleResourceSelected; 
            private set => SetProperty(ref _isSingleResourceSelected, value); 
        }
        public bool IsSingleRecipeSelected
        {
            get => _isSingleRecipeSelected;
            private set => SetProperty(ref _isSingleRecipeSelected, value);
        }
        public bool IsSingleComponentSelected
        {
            get => _isSingleComponentSelected;
            private set => SetProperty(ref _isSingleComponentSelected, value);
        }

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
            ClearSelectedRecipes();
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

        public void AddRecipeToSelected(RecipeItemViewModel recipe)
        {
            if (Recipes.Contains(recipe!)) return;
            Recipes.Add(recipe);
        }

        public void ClearSelectedRecipes()
        {
            Resources.Clear();
            ClearSelectedComponents();
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
    }
}
