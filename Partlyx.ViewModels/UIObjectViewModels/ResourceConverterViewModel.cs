using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ResourceConverterViewModel : PartlyxObservable
    {
        private readonly IEventBus _bus;
        private readonly IDialogService _dialogService;
        private readonly IVMPartsStore _store;
        private readonly VMComponentsGraphs _graphs;
        private readonly IComponentPathUiStateService _pathItemsStateService;
        private readonly ThrottledInvoker _throttled;

        public ResourceConverterViewModel(IEventBus bus, IDialogService dialogService, IVMPartsStore store, VMComponentsGraphs graphs, IComponentPathUiStateService pathsStateService)
        {
            _bus = bus;
            _dialogService = dialogService;
            _store = store;
            _graphs = graphs;
            _pathItemsStateService = pathsStateService;

            _throttled = new ThrottledInvoker(TimeSpan.FromMilliseconds(200));

            // Subscriptions
            Disposables.Add(bus.Subscribe<RecipeComponentUpdatedEvent>(ev =>
            {
                if (ev.ChangedProperties?.ContainsKey(nameof(RecipeComponentViewModel.Quantity)) ?? false)
                    UpdatePathAmounts();
            }));
            Disposables.Add(bus.Subscribe<ComponentGraphsUpdatedEvent>(_ => UpdateResults()));
        }
        // <-- Converting data -->
        private ResourceViewModel? _inputResource;
        public ResourceViewModel? InputResource { get => _inputResource; set { SetProperty(ref _inputResource, value); UpdateResults(); } }

        private ResourceViewModel? _outputResource;
        public ResourceViewModel? OutputResource { get => _outputResource; set { SetProperty(ref _outputResource, value); UpdateResults(); } }

        private double _inputAmount = 1.0;
        public double InputAmount { get => _inputAmount; set { SetProperty(ref _inputAmount, value); UpdatePathAmounts(); } }
        private double _outputAmount = 1.0;

        public double OutputAmount { get => _outputAmount; set { SetProperty(ref _outputAmount, value); UpdatePathAmounts(); } }

        [RelayCommand]
        public async Task SelectInput()
        {
            var selected = await SelectResource();

            if (!selected.IsCancelled)
                InputResource = selected.Resource;
        }
        [RelayCommand]
        public async Task SelectOutput()
        {
            var selected = await SelectResource();

            if (!selected.IsCancelled)
                OutputResource = selected.Resource;
        }

        private async Task<(ResourceViewModel? Resource, bool IsCancelled)> SelectResource()
        {
            var allTheResourcesList = _store.Resources.Values.ToList();
            var dialogVM = new ResourcesSelectionViewModel(_dialogService, new IsolatedSelectedParts())
            {
                EnableMultiSelect = false,
                Items = new ObservableCollection<ResourceViewModel>(allTheResourcesList),
                IsSelectionNecessaryToConfirm = true
            };

            var result = await _dialogService.ShowDialogAsync(dialogVM);

            if (result is not ISelectedParts selected) return (null, true);
            var resource = selected.GetSingleResourceOrNull()!;

            return (resource, false);
        }

        // <-- Converting options -->
        private bool _isCalculatingFromInput = true;
        public bool IsCalculatingFromInput { get => _isCalculatingFromInput; set { SetProperty(ref _isCalculatingFromInput, value); UpdatePathAmounts(); } }
        private bool _isCalculatingFromOutput = false;
        public bool IsCalculatingFromOutput { get => _isCalculatingFromOutput; set { SetProperty(ref _isCalculatingFromOutput, value); UpdatePathAmounts(); } }
        // <-- Converting results -->
        public ObservableCollection<RecipeComponentPathItem> AvailableConversions { get; set; } = new();

        public void UpdateResults() => _throttled.InvokeAsync(UpdateResultsPrivate);
        private void UpdateResultsPrivate()
        {
            ClearResults();

            if (InputResource == null || OutputResource == null)
                return;

            var paths = _graphs.FindPathsBetweenResources(InputResource, OutputResource);
            var pathItems = paths.Select(p => new RecipeComponentPathItem(p, _pathItemsStateService));
            AvailableConversions.AddRange(pathItems);

            UpdatePathAmounts();
        }
        private void UpdatePathAmounts()
        {
            var amountArgument = IsCalculatingFromOutput ? OutputAmount : InputAmount;
            foreach (var path in AvailableConversions)
                path.UpdateSums(amountArgument, IsCalculatingFromOutput);

            var paths = AvailableConversions.Select(pi => pi.Path);
            var ev = new OnComponentPathAmountsUpdatedEvent(paths.ToHashSet());
            _bus.Publish(ev);
        }
        private void ClearResults()
        {
            AvailableConversions.Clear();
        }
    }

    public record OnComponentPathAmountsUpdatedEvent(HashSet<RecipeComponentPath> ChangedPaths);
}
