using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIStates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public record TreeSearchQueryEvent(string queryText, PartTypeEnumVM? searchablePartType = null);

    public partial class PartsTreeViewModel : PartlyxObservable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _initializationFinishedSubscription;
        private readonly IDisposable _fileClearedSubscription;
        private readonly IDisposable _treeSearchQuerySubscription;

        //
        public IGlobalSelectedParts SelectedParts { get; }
        public IGlobalFocusedPart FocusedPart { get; }
        public IResourceSearchService Search { get; }
        public PartsServiceViewModel Service { get; }
        public PartsTreeViewContextMenuCommands ContextMenuCommands { get; }

        private IGlobalResourcesVMContainer _resourcesContainer { get; }
        public ObservableCollection<ResourceViewModel> Resources => _resourcesContainer.Resources;

        private IGlobalRecipesVMContainer _recipesContainer { get; }
        public ObservableCollection<RecipeViewModel> Recipes => _recipesContainer.Recipes;
        public ObservableCollection<IVMPart> SelectedPartsCollection { get; } = new();
        public PartsSelectionState SelectedPartsDetails { get; }

        private bool _allowHotkeys = true;
        public bool AllowHotkeys { get => _allowHotkeys; private set => SetProperty(ref _allowHotkeys, value); }
        private void UpdateAllowHotkeys()
        {
            AllowHotkeys = _hotkeysBlockElementsAmount == 0;
        }
        private int _hotkeysBlockElementsAmount;
        public int HotkeysBlockElementsAmount 
        {
            get => _hotkeysBlockElementsAmount;
            set
            {
                if (SetProperty(ref _hotkeysBlockElementsAmount, value))
                    UpdateAllowHotkeys();
            }
        }

        public PartsTreeViewModel(IGlobalResourcesVMContainer grvmc, IGlobalRecipesVMContainer grc, IGlobalSelectedParts sp, IGlobalFocusedPart fp, IEventBus bus, IVMPartsFactory vmpf,
                IVMPartsStore vmps, IResourceSearchService rss, PartsServiceViewModel service)
        {
            _resourcesContainer = grvmc;
            _recipesContainer = grc;
            _partsFactory = vmpf;
            _store = vmps;
            _bus = bus;

            SelectedParts = sp;
            FocusedPart = fp;
            Search = rss;
            Service = service;

            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
            _initializationFinishedSubscription = bus.Subscribe<PartsVMInitializationFinishedEvent>((ev) => UpdateList(), true);
            _fileClearedSubscription = bus.Subscribe<FileClearedEvent>((ev) => { Resources.Clear(); Recipes.Clear(); }, true);
            _treeSearchQuerySubscription = bus.Subscribe<TreeSearchQueryEvent>(SearchQueryHandler);

            SelectedPartsDetails = new PartsSelectionState(SelectedPartsCollection);
            ContextMenuCommands = new PartsTreeViewContextMenuCommands(this);
        }

        private void AddFromDto(ResourceDto dto)
        {
            var resourceVM = _partsFactory.GetOrCreateResourceVM(dto);
            _resourcesContainer.AddResource(resourceVM);
        }

        private void OnResourceCreated(ResourceCreatedEvent ev)
        {
            AddFromDto(ev.Resource);
        }

        private void OnResourceDeleted(ResourceDeletedEvent ev)
        {
            var resourceVM = Resources.FirstOrDefault(c => c.Uid == ev.ResourceUid);
            if (resourceVM != null)
            {
                _resourcesContainer.RemoveResource(resourceVM);
                resourceVM.Dispose();
            }
        }

        public void Dispose()
        {
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
            _initializationFinishedSubscription.Dispose();
            _fileClearedSubscription.Dispose();
            _treeSearchQuerySubscription.Dispose();
        }

        public void UpdateList()
        {
            _resourcesContainer.ClearResources();
            _recipesContainer.ClearRecipes();

            foreach (var resource in _store.Resources.Values)
                _resourcesContainer.AddResource(resource);

            foreach (var recipe in _store.Recipes.Values)
                _recipesContainer.AddRecipe(recipe);
        }

        private void SearchQueryHandler(TreeSearchQueryEvent ev)
        {
            switch (ev.searchablePartType)
            {
                case PartTypeEnumVM.Recipe:
                    ExpandAllTheResources();
                    break;
                default:
                    ExpandAll();
                    break;
            }

            Search.SearchText = ev.queryText;
        }

        [RelayCommand]
        public void CallSearch(string queryText)
        {
            Search.SearchText = queryText;
        }

        [RelayCommand]
        public void ClearSearch()
        {
            if (Search.SearchText == string.Empty) return;

            CollapseAll();
            Search.SearchText = string.Empty;
        }

        [RelayCommand]
        public void ExpandAllTheResources()
        {
            var ev = new SetAllTheResourceItemsExpandedEvent(true);
            _bus.Publish(ev);
        }
        [RelayCommand]
        public void CollapseAllTheResources()
        {
            var ev = new SetAllTheResourceItemsExpandedEvent(false);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void ExpandAllTheRecipes()
        {
            var ev = new SetAllTheRecipeItemsExpandedEvent(true);
            _bus.Publish(ev);
        }
        [RelayCommand]
        public void CollapseAllTheRecipes()
        {
            var ev = new SetAllTheRecipeItemsExpandedEvent(false);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void ExpandAll()
        {
            ExpandAllTheResources();

            ExpandAllTheRecipes();
        }

        [RelayCommand]
        public void CollapseAll()
        {
            CollapseAllTheResources();

            CollapseAllTheRecipes();
        }

        [RelayCommand(CanExecute = nameof(AllowHotkeys))]
        public void ActivateHotkey(ICommand hotkeyCommand)
        {
             hotkeyCommand.Execute(null);
        }

        [RelayCommand]
        public async Task CreateChildForSelected()
        {
            // If any recipe selected, we want to know what components user wants to create for them
            ResourceViewModel[]? resourcesForComponentsCreate = null;
            if (SelectedPartsCollection.Any(p => p is RecipeViewModel))
            {
                var selected = await Service.ComponentService.ShowComponentCreateMenuAsync();
                if (selected != null)
                    resourcesForComponentsCreate = selected.Resources.ToArray();
            }

            foreach (var part in SelectedPartsCollection)
            {
                if (part is ResourceViewModel resource)
                {
                    await Service.RecipeService.CreateRecipeAsync(resource);
                }
                else if (part is RecipeViewModel recipe && resourcesForComponentsCreate != null)
                {
                    await Service.ComponentService.CreateComponentsFromAsync(recipe, resourcesForComponentsCreate);
                }
            }
        }
    }

    public partial class PartsTreeViewContextMenuCommands : ContextMenuCommands, IDisposable
    {
        private readonly PartsServiceViewModel _service;
        private readonly ObservableCollection<IVMPart> _selected;

        public PartsTreeViewModel PartsTree;

        [ObservableProperty] private bool _allowCreateResource;
        [ObservableProperty] private bool _allowCreateRecipe;
        [ObservableProperty] private bool _allowCreateComponent;
        [ObservableProperty] private bool _allowQuantifyRecipe;
        [ObservableProperty] private bool _allowMergeRecipe;
        [ObservableProperty] private bool _allowDuplicate;
        [ObservableProperty] private bool _allowToggleFocus;
        [ObservableProperty] private bool _allowExpandBranch;
        [ObservableProperty] private bool _allowCollapseBranch;
        [ObservableProperty] private bool _allowFindResourceInTree;
        [ObservableProperty] private bool _allowFindRecipeInTree;
        [ObservableProperty] private bool _allowFindComponentInTree;
        [ObservableProperty] private bool _allowDelete;

        public PartsTreeViewContextMenuCommands(PartsTreeViewModel partsTree)
        {
            PartsTree = partsTree;

            _service = PartsTree.Service;
            _selected = PartsTree.SelectedPartsCollection;
            _details = PartsTree.SelectedPartsDetails;

            _details.FlagsUpdated += UpdateAllowed;
            UpdateAllowed();
        }

        public override void AllowAll()
        {
            AllowToggleFocus = AllowExpandBranch = AllowCollapseBranch = AllowFindResourceInTree = AllowFindRecipeInTree = AllowFindComponentInTree = AllowDelete = true;
        }

        private readonly PartsSelectionState _details;
        public override void UpdateAllowed()
        {
            AllowDelete = AllowDuplicate = _details.HasAnything;
            AllowExpandBranch = AllowCollapseBranch = _details.HasAnything && !_details.HasOnlyComponents;
            AllowDuplicate = _details.HasOnlyResources || _details.HasOnlyRecipes || _details.HasOnlyComponents;

            AllowCreateResource = !_details.HasAnything;
            AllowCreateRecipe = _details.HasOnlyResources;
            AllowCreateComponent = _details.HasOnlyRecipes;

            bool singleSelect = _details.PartsCount == 1;
            AllowToggleFocus = singleSelect;

            AllowQuantifyRecipe = _details.HasOnlyRecipes;
            AllowMergeRecipe = _details.HasOnlyRecipes;

            AllowFindResourceInTree = (_details.HasOnlyResources || _details.HasOnlyComponents) && singleSelect;
            AllowFindRecipeInTree = _details.HasOnlyRecipes && singleSelect;
            AllowFindComponentInTree = _details.HasOnlyComponents && singleSelect;
        }

        new public void Dispose()
        {
            base.Dispose();

            _details.FlagsUpdated -= UpdateAllowed;
        }

        [RelayCommand]
        public async Task CreateResourceAsync() => await _service.ResourceService.CreateResourceAsync();
        [RelayCommand]
        public async Task CreateRecipeAsync()
        {
            foreach (var part in _selected)
            {
                if (part is ResourceViewModel resource)
                    await _service.RecipeService.CreateRecipeAsync(resource);
            }
        }
        [RelayCommand]
        public async Task CreateComponentAsync()
        {
            var recipes = _selected.Where(p => p is RecipeViewModel).Cast<RecipeViewModel>();
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
            foreach (var part in _selected)
            {
                await _service.Duplicate(part);
            }
        }

        [RelayCommand]
        public async Task CreateQuantifiedRecipeAsync()
        {
            foreach (var part in _selected)
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
            foreach (var part in _selected)
            {
                if (part is RecipeViewModel recipe)
                {
                    await _service.ComponentService.MergeSameComponentsAsync(recipe);
                }
            }
        }

        [RelayCommand]
        public void ToggleFocus() => _selected.SingleOrDefault()?.UiItem.ToggleGlobalFocus();

        [RelayCommand]
        public void ExpandBranch()
        {
            foreach (var part in _selected)
            {
                part.UiItem.Expand();
                if (part is ResourceViewModel resource)
                    foreach (var recipe in resource.Recipes)
                        recipe.UiItem.Expand();
            }
        }

        [RelayCommand]
        public void CollapseBranch()
        {
            foreach (var part in _selected)
            {
                part.UiItem.Collapse();
                if (part is ResourceViewModel resource)
                    foreach (var recipe in resource.Recipes)
                        recipe.UiItem.Collapse();
            }
        }

        [RelayCommand]
        public void FindResourceInTree()
        {
            var singlePart = _selected.SingleOrDefault();
            if (singlePart is ResourceViewModel resource)
                resource.UiItem.FindInTree();
            else if (singlePart is RecipeComponentViewModel component)
                component.UiItem.FindResourceInTree();
        }

        [RelayCommand]
        public void FindRecipeInTree()
        {
            var singlePart = _selected.SingleOrDefault();
            if (singlePart is RecipeViewModel recipe)
                recipe.UiItem.FindInTree();
        }
        [RelayCommand]
        public void FindComponentInTree()
        {
            var singlePart = _selected.SingleOrDefault();
            if (singlePart is RecipeComponentViewModel component)
                component.UiItem.FindInTree();
        }

        [RelayCommand]
        public async Task Delete()
        {
            var selectedCopy = new ObservableCollection<IVMPart>(_selected);

            foreach (var part in selectedCopy)
            {
                await _service.RemoveAsync(part);
            }
        }
    }
}
