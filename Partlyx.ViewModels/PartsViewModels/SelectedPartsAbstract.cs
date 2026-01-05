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
        private bool _isPartsSelected;

        [ObservableProperty]
        private ResourceViewModel? _singleResourceOrNull;
        [ObservableProperty]
        private RecipeViewModel? _singleRecipeOrNull;
        [ObservableProperty]
        private RecipeComponentViewModel? _singleComponentOrNull;

        private readonly HashSet<ResourceViewModel> _resourcesSet = new();
        private readonly HashSet<RecipeViewModel> _recipesSet = new();
        private readonly HashSet<RecipeComponentViewModel> _componentsSet = new();

        private readonly ObservableCollection<ResourceViewModel> _resourcesObservable = new();
        private readonly ObservableCollection<RecipeViewModel> _recipesObservable = new();
        private readonly ObservableCollection<RecipeComponentViewModel> _componentsObservable = new();

        public ReadOnlyObservableCollection<ResourceViewModel> Resources { get; }
        public ReadOnlyObservableCollection<RecipeViewModel> Recipes { get; }
        public ReadOnlyObservableCollection<RecipeComponentViewModel> Components { get; }

        public SelectedPartsAbstract()
        {
            Resources = new ReadOnlyObservableCollection<ResourceViewModel>(_resourcesObservable);
            Recipes = new ReadOnlyObservableCollection<RecipeViewModel>(_recipesObservable);
            Components = new ReadOnlyObservableCollection<RecipeComponentViewModel>(_componentsObservable);

            _resourcesObservable.CollectionChanged += (sender, evInfo) =>
            {
                IsSingleResourceSelected = _resourcesObservable.Count == 1;
                SingleResourceOrNull = GetSingleResourceOrNull();

                IsResourcesSelected = _resourcesObservable.Count > 0;
                IsPartsSelected = IsResourcesSelected || IsRecipesSelected || IsComponentsSelected;
                SelectedResourcesChangedHandler(sender, evInfo);
            };
            _recipesObservable.CollectionChanged += (sender, evInfo) =>
            {
                IsSingleRecipeSelected = _recipesObservable.Count == 1;
                SingleRecipeOrNull = GetSingleRecipeOrNull();

                IsRecipesSelected = _recipesObservable.Count > 0;
                IsPartsSelected = IsResourcesSelected || IsRecipesSelected || IsComponentsSelected;
                SelectedRecipesChangedHandler(sender, evInfo);
            };
            _componentsObservable.CollectionChanged += (sender, evInfo) =>
            {
                IsSingleComponentSelected = _componentsObservable.Count == 1;
                SingleComponentOrNull = GetSingleComponentOrNull();

                IsComponentsSelected = _componentsObservable.Count > 0;
                IsPartsSelected = IsResourcesSelected || IsRecipesSelected || IsComponentsSelected;
                SelectedComponentsChangedHandler(sender, evInfo);
            };
        }

        protected virtual void SelectedResourcesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedRecipesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }
        protected virtual void SelectedComponentsChangedHandler(object? sender, NotifyCollectionChangedEventArgs args) { }

        #region Resource methods
        public void SelectSingleResource(ResourceViewModel resource)
        {
            if (_resourcesSet.Count == 1 && _resourcesSet.Contains(resource)) return;

            ClearSelectedResources();

            _resourcesSet.Add(resource);
            _resourcesObservable.Add(resource);
        }

        public void AddResourceToSelected(ResourceViewModel resource)
        {
            if (_resourcesSet.Add(resource))
                _resourcesObservable.Add(resource);
        }

        public void ClearSelectedResources()
        {
            _resourcesSet.Clear();
            _resourcesObservable.Clear();
        }

        public void RemoveResourceFromSelected(ResourceViewModel resource)
        {
            if (_resourcesSet.Remove(resource))
                _resourcesObservable.Remove(resource);
        }

        public ResourceViewModel? GetSingleResourceOrNull()
        {
            bool isResourceSingle = Resources.Count == 1;
            return isResourceSingle ? Resources.Single() : null;
        }
        #endregion

        #region Recipe methods
        public void SelectSingleRecipe(RecipeViewModel recipe)
        {
            if (_recipesSet.Count == 1 && _recipesSet.Contains(recipe)) return;

            ClearSelectedRecipes();

            _recipesSet.Add(recipe);
            _recipesObservable.Add(recipe);
        }

        public void SelectSingleRecipeAncestor(RecipeViewModel recipe)
        {
            if (recipe.LinkedParentResource?.Value is ResourceViewModel parentResource)
                SelectSingleResource(parentResource);
            else
                ClearSelectedResources();
        }

        public void AddRecipeToSelected(RecipeViewModel recipe)
        {
            if (_recipesSet.Add(recipe))
                _recipesObservable.Add(recipe);
        }

        public void ClearSelectedRecipes()
        {
            _recipesSet.Clear();
            _recipesObservable.Clear();
        }

        public void RemoveRecipeFromSelected(RecipeViewModel recipe)
        {
            if (_recipesSet.Remove(recipe))
                _recipesObservable.Remove(recipe);
        }

        public RecipeViewModel? GetSingleRecipeOrNull()
        {
            bool isRecipeSingle = Recipes.Count == 1;
            return isRecipeSingle ? Recipes.Single() : null;
        }
        #endregion

        #region Component methods
        public void SelectSingleComponent(RecipeComponentViewModel component)
        {
            if (_componentsSet.Count == 1 && _componentsSet.Contains(component)) return;

            ClearSelectedComponents();

            _componentsSet.Add(component);
            _componentsObservable.Add(component);
        }

        public void SelectSingleComponentAncestors(RecipeComponentViewModel component)
        {
            if (component.LinkedParentRecipe?.Value is RecipeViewModel parentRecipe)
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

        public void AddComponentToSelected(RecipeComponentViewModel component)
        {
            if (_componentsSet.Add(component))
                _componentsObservable.Add(component);
        }

        public void ClearSelectedComponents()
        {
            _componentsSet.Clear();
            _componentsObservable.Clear();
        }

        public void RemoveComponentFromSelected(RecipeComponentViewModel component)
        {
            if (_componentsSet.Remove(component))
                _componentsObservable.Remove(component);
        }

        public RecipeComponentViewModel? GetSingleComponentOrNull()
        {
            bool isComponentSingle = Components.Count == 1;
            return isComponentSingle ? Components.Single() : null;
        }
        #endregion

        #region Generic / any parts
        public void SelectSinglePart(IVMPart part)
        {
            if (part is ResourceViewModel resource)
            {
                SelectSingleResource(resource);
            }
            else if (part is RecipeViewModel recipe)
            {
                SelectSingleRecipe(recipe);
            }
            else if (part is RecipeComponentViewModel component)
            {
                SelectSingleComponent(component);
            }
        }

        public void AddPartToSelected(IVMPart part)
        {
            if (part is ResourceViewModel resource)
            {
                AddResourceToSelected(resource);
            }
            else if (part is RecipeViewModel recipe)
            {
                AddRecipeToSelected(recipe);
            }
            else if (part is RecipeComponentViewModel component)
            {
                AddComponentToSelected(component);
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
