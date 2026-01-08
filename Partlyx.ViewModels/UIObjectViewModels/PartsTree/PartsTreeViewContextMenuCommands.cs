using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsTreeViewContextMenuCommands : ContextMenuCommands, IDisposable
    {
        private readonly PartsServiceViewModel _service;

        public PartsTreeViewModel PartsTree { get; }

        [ObservableProperty] private bool _allowCreateResource;
        [ObservableProperty] private bool _allowCreateRecipeForResource;
        [ObservableProperty] private bool _allowCreateRecipe;
        [ObservableProperty] private bool _allowCreateComponent;
        [ObservableProperty] private bool _allowQuantifyRecipe;
        [ObservableProperty] private bool _allowMergeRecipe;
        [ObservableProperty] private bool _allowDuplicate;
        [ObservableProperty] private bool _allowToggleFocus;
        [ObservableProperty] private bool _allowExpandBranch;
        [ObservableProperty] private bool _allowCollapseBranch;
        [ObservableProperty] private bool _allowFindResource;
        [ObservableProperty] private bool _allowFindRecipe;
        [ObservableProperty] private bool _allowFindComponent;
        [ObservableProperty] private bool _allowDelete;

        public PartsTreeViewContextMenuCommands(PartsTreeViewModel partsTree)
        {
            PartsTree = partsTree;

            _service = PartsTree.Service;
            // Selected tab subscription
            Disposables.Add(PartsTree.WhenAnyValue(t => t.SelectedTab).Subscribe(_ => OnTabChanged()));

            OnTabChanged();
        }

        private ICollection<object> _currentSelected;
        private PartsSelectionState _currentDetails;
        private void OnTabChanged()
        {
            if (_currentDetails != null)
                _currentDetails.FlagsUpdated -= UpdateAllowed;

            if (PartsTree.SelectedTab == PartsTreeViewModel.TabEnum.Resources)
            {
                _currentSelected = PartsTree.ResourcesTab.LocalSelectedPartsCollection;
                _currentDetails = PartsTree.ResourcesTab.LocalSelectedPartsDetails;
            }
            else if (PartsTree.SelectedTab == PartsTreeViewModel.TabEnum.Recipes)
            {
                _currentSelected = PartsTree.RecipesTab.LocalSelectedPartsCollection;
                _currentDetails = PartsTree.RecipesTab.LocalSelectedPartsDetails;
            }

            if (_currentDetails != null)
                _currentDetails.FlagsUpdated += UpdateAllowed;

            UpdateAllowed();
        }

        public override void AllowAll()
        {
            AllowCreateResource = AllowCreateRecipeForResource = AllowCreateRecipe = AllowCreateComponent = AllowQuantifyRecipe = AllowMergeRecipe = AllowDuplicate = AllowToggleFocus
                = AllowExpandBranch = AllowCollapseBranch = AllowFindResource = AllowFindRecipe = AllowFindComponent = AllowDelete = true;
        }

        public override void UpdateAllowed()
        {
            AllowDelete = AllowDuplicate = _currentDetails.HasAnything;
            AllowExpandBranch = AllowCollapseBranch = _currentDetails.HasAnything && !_currentDetails.HasOnlyComponents;
            AllowDuplicate = _currentDetails.HasOnlyResources || _currentDetails.HasOnlyRecipes || _currentDetails.HasOnlyComponents;

            AllowCreateResource = !_currentDetails.HasAnything && PartsTree.SelectedTab == PartsTreeViewModel.TabEnum.Resources;
            AllowCreateRecipeForResource = _currentDetails.HasOnlyResources;
            AllowCreateRecipe = !_currentDetails.HasAnything && PartsTree.SelectedTab == PartsTreeViewModel.TabEnum.Recipes;
            AllowCreateComponent = _currentDetails.HasOnlyRecipes;

            bool singleSelect = _currentDetails.PartsCount == 1;
            AllowToggleFocus = singleSelect;

            AllowQuantifyRecipe = _currentDetails.HasOnlyRecipes;
            AllowMergeRecipe = _currentDetails.HasOnlyRecipes;

            AllowFindResource = (_currentDetails.HasOnlyResources || _currentDetails.HasOnlyComponents) && singleSelect;
            AllowFindRecipe = _currentDetails.HasOnlyRecipes && singleSelect;
            AllowFindComponent = _currentDetails.HasOnlyComponents && singleSelect;
        }

        new public void Dispose()
        {
            base.Dispose();

            _currentDetails.FlagsUpdated -= UpdateAllowed;
        }

        [RelayCommand]
        public async Task CreateResourceAsync() => await _service.ResourceService.CreateResourceAsync();
        [RelayCommand]
        public async Task CreateRecipeForResourceAsync()
        {
            foreach (var part in _currentSelected)
            {
                if (part is ResourceViewModel resource)
                    await _service.RecipeService.CreateRecipeAsync(resource);
            }
        }
        [RelayCommand]
        public async Task CreateRecipeAsync()
            => await _service.RecipeService.CreateRecipeAsync();
        [RelayCommand]
        public async Task CreateComponentAsync()
        {
            var recipes = _currentSelected.Where(p => p is RecipeViewModel).Cast<RecipeViewModel>();
            if (recipes.Any())
            {
                var selected = await _service.ComponentService.ShowComponentCreateMenuAsync();
                ResourceViewModel[]? resourcesForComponentsCreate = null;
                if (selected != null)
                    resourcesForComponentsCreate = selected.Resources.ToArray();

                if (resourcesForComponentsCreate == null)
                    return;

                foreach (var recipe in recipes)
                    await _service.ComponentService.CreateComponentsFromAsync(recipe, resourcesForComponentsCreate);
            }
        }

        [RelayCommand]
        public async Task DuplicateAsync()
        {
            foreach (var part in _currentSelected.OfType<IVMPart>())
            {
                await _service.Duplicate(part);
            }
        }

        [RelayCommand]
        public async Task CreateQuantifiedRecipeAsync()
        {
            foreach (var part in _currentSelected)
            {
                if (part is  RecipeViewModel recipe)
                {
                    await _service.CreateQuantifiedCloneAsync(recipe);
                }
            }
        }
        [RelayCommand]
        public async Task MergeComponentsForRecipeAsync()
        {
            foreach (var recipe in _currentSelected.OfType<RecipeViewModel>().ToList())
            {
                await _service.ComponentService.MergeSameComponentsAsync(recipe);
            }
        }

        [RelayCommand]
        public void ToggleFocus() 
            => _currentSelected
            .OfType<IVMPart>()
            .SingleOrDefault()
            ?.UiItem.ToggleGlobalFocus();

        [RelayCommand]
        public void ExpandBranch()
        {
            foreach (var part in _currentSelected.OfType<IVMPart>())
            {
                part.UiItem.Expand();
            }
        }

        [RelayCommand]
        public void CollapseBranch()
        {
            foreach (var part in _currentSelected.OfType<IVMPart>())
            {
                part.UiItem.Collapse();
            }
        }

        [RelayCommand]
        public void FindResource()
        {
            var singlePart = _currentSelected.SingleOrDefault();
            if (singlePart is ResourceViewModel resource)
                resource.UiItem.FindInTree();
            else if (singlePart is RecipeComponentViewModel component)
                component.UiItem.FindResourceInTree();
        }

        [RelayCommand]
        public void FindRecipe()
        {
            var singlePart = _currentSelected.SingleOrDefault();
            if (singlePart is RecipeViewModel recipe)
                recipe.UiItem.FindInTree();
        }
        [RelayCommand]
        public void FindComponent()
        {
            var singlePart = _currentSelected.SingleOrDefault();
            if (singlePart is RecipeComponentViewModel component)
                component.UiItem.FindInTree();
        }

        [RelayCommand]
        public async Task Delete()
        {
            var selectedCopy = new ObservableCollection<IVMPart>(_currentSelected.OfType<IVMPart>());

            foreach (var part in selectedCopy)
            {
                await _service.RemoveAsync(part);
            }
        }
    }
}
