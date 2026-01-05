using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsTreeRecipesViewModel : PartlyxObservable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;
        public PartsTreeViewModel? ParentTreeService { get; set; }

        public IGlobalSelectedParts SelectedParts { get; }
        public IIsolatedSelectedParts LocalSelectedParts { get; }
        public IGlobalFocusedElementContainer FocusedPart { get; }
        public IRecipeSearchService Search { get; }
        public PartsServiceViewModel Service { get; }

        private IGlobalRecipesVMContainer _recipesContainer { get; }
        public ObservableCollection<RecipeViewModel> Recipes => _recipesContainer.Recipes;

        public ObservableCollection<object> LocalSelectedPartsCollection { get; } = new();
        public PartsSelectionState LocalSelectedPartsDetails { get; }

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

        public PartsTreeRecipesViewModel(IGlobalRecipesVMContainer grc, IGlobalSelectedParts sp, IIsolatedSelectedParts isp, IGlobalFocusedElementContainer fp, IEventBus bus, IVMPartsFactory vmpf,
                IVMPartsStore vmps, IRecipeSearchService rcss, PartsServiceViewModel service)
        {
            _recipesContainer = grc;
            _partsFactory = vmpf;
            _store = vmps;
            _bus = bus;

            SelectedParts = sp;
            LocalSelectedParts = isp;
            FocusedPart = fp;
            Search = rcss;
            Service = service;

            var selectionHelper = new SelectedPartsObserveHelper(LocalSelectedParts, LocalSelectedPartsCollection);
            Disposables.Add(selectionHelper);

            // Recipe added subscription
            Disposables.Add(bus.Subscribe<RecipeCreatedEvent>(OnRecipeCreated, true));
            // Recipe removed subscription
            Disposables.Add(bus.Subscribe<RecipeDeletedEvent>(OnRecipeDeleted, true));
            // Parts initialization finished subscription
            Disposables.Add(bus.Subscribe<PartsVMInitializationFinishedEvent>((ev) => UpdateList(), true));
            // All the parts removed subscription
            Disposables.Add(bus.Subscribe<FileClearedEvent>((ev) => { Recipes.Clear(); }, true));
            // Tree search query handle subscription
            Disposables.Add(bus.Subscribe<TreeSearchQueryEvent>(SearchQueryHandler));

            LocalSelectedPartsDetails = new PartsSelectionState(LocalSelectedPartsCollection);
        }

        private void OnRecipeCreated(RecipeCreatedEvent ev)
        {
            var recipeVM = _partsFactory.GetOrCreateRecipeVM(ev.Recipe);
            _recipesContainer.AddRecipe(recipeVM);
        }

        private void OnRecipeDeleted(RecipeDeletedEvent ev)
        {
            var recipeVM = Recipes.FirstOrDefault(c => c.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                _recipesContainer.RemoveRecipe(recipeVM);
                recipeVM.Dispose();
            }
        }

        public void UpdateList()
        {
            _recipesContainer.ClearRecipes();

            foreach (var recipe in _store.Recipes.Values)
                _recipesContainer.AddRecipe(recipe);
        }

        private void SearchQueryHandler(TreeSearchQueryEvent ev)
        {
            if (ev.searchablePartType == PartTypeEnumVM.Recipe)
            {
                ExpandAll();
            }

            Search.SearchText = ev.queryText;
        }
        [RelayCommand]
        public async Task CreateInputAsync(RecipeViewModel parent)
            => await Service.ComponentService.CreateInputAsync(parent);
        [RelayCommand]
        public async Task CreateOutputAsync(RecipeViewModel parent)
            => await Service.ComponentService.CreateOutputAsync(parent);
        [RelayCommand]
        public async Task CreateRecipeAsync()
        {
            await Service.RecipeService.CreateRecipeAsync();
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
        public void ExpandAll()
        {
            var ev = new SetAllTheRecipeItemsExpandedEvent(true);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void CollapseAll()
        {
            var ev = new SetAllTheRecipeItemsExpandedEvent(false);
            _bus.Publish(ev);
        }

        [RelayCommand(CanExecute = nameof(AllowHotkeys))]
        public void ActivateHotkey(ICommand hotkeyCommand)
        {
             hotkeyCommand.Execute(null);
        }
    }
}
