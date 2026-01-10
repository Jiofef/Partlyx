using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
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
    public partial class PartsTreeResourcesViewModel : PartlyxObservable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;

        public PartsTreeViewModel? ParentTreeService { get; set; }
        public IGlobalSelectedParts SelectedParts { get; }
        public IIsolatedSelectedParts LocalSelectedResources { get; }
        public IGlobalFocusedElementContainer FocusedPart { get; }
        public IResourceSearchService Search { get; }
        public PartsServiceViewModel Service { get; }
        private IGlobalResourcesVMContainer _resourcesContainer { get; }
        public ObservableCollection<ResourceViewModel> Resources => _resourcesContainer.Resources;

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

        public PartsTreeResourcesViewModel(IGlobalResourcesVMContainer grvmc, IGlobalSelectedParts sp, IIsolatedSelectedParts isp, IGlobalFocusedElementContainer fp, IEventBus bus, IVMPartsFactory vmpf,
                IVMPartsStore vmps, IResourceSearchService rss, PartsServiceViewModel service)
        {
            _resourcesContainer = grvmc;
            _partsFactory = vmpf;
            _store = vmps;
            _bus = bus;

            SelectedParts = sp;
            LocalSelectedResources = isp;
            FocusedPart = fp;
            Search = rss;
            Service = service;

            var selectionHelper = new SelectedPartsObserveHelper(LocalSelectedResources, LocalSelectedPartsCollection);
            Disposables.Add(selectionHelper);

            // Recipe add subscription
            Disposables.Add(bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true));
            // Recipe remove subscription
            Disposables.Add(bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true));
            // Parts initialization finished subscription
            Disposables.Add(bus.Subscribe<PartsVMInitializationFinishedEvent>((ev) => UpdateList(), true));
            // All the parts removed subscription
            Disposables.Add(bus.Subscribe<FileClearedEvent>((ev) => { Resources.Clear(); }, true));
            // Tree search query handle subscription
            Disposables.Add(bus.Subscribe<TreeSearchQueryEvent>(SearchQueryHandler));

            LocalSelectedPartsDetails = new PartsSelectionState(LocalSelectedPartsCollection);
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

        public void UpdateList()
        {
            _resourcesContainer.ClearResources();

            foreach (var resource in _store.Resources.Values)
                _resourcesContainer.AddResource(resource);
        }

        private void SearchQueryHandler(TreeSearchQueryEvent ev)
        {
            if (ev.searchablePartType != PartTypeEnumVM.Recipe)
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
            _bus.Publish(new SetAllTheResourceItemsExpandedEvent(true));
            _bus.Publish(new SetAllTheRecipeItemsExpandedEvent(true));
        }

        [RelayCommand]
        public void CollapseAll()
        {
            _bus.Publish(new SetAllTheRecipeItemsExpandedEvent(false));
            _bus.Publish(new SetAllTheResourceItemsExpandedEvent(false));
        }

        [RelayCommand(CanExecute = nameof(AllowHotkeys))]
        public void ActivateHotkey(ICommand hotkeyCommand)
        {
             hotkeyCommand.Execute(null);
        }

        [RelayCommand]
        public async Task CreateRecipeFromResourcesMenuAsync(ResourceViewModel parentResource)
        {
            await Service.RecipeService.CreateRecipeAsync(parentResource);
        }
        [RelayCommand]
        public async Task FindRecipeFromResourcesMenuAsync(RecipeViewModel recipe)
        {
            // To do
            await Task.Delay(1);
        }

        [RelayCommand]
        public async Task CreateResourceAsync()
        {
            await Service.ResourceService.CreateResourceAsync();
        }
    }
}
