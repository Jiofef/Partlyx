using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Interfaces;
using ReactiveUI;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsGraphViewModel : ObservableObject, IDisposable
    {
        public IGlobalSelectedParts SelectedParts { get; }

        public IMainWindowController MainWindowController { get; }

        private List<IDisposable> _subscriptions = new();

        // Pan/zoom
        public PanAndZoomControllerViewModel PanAndZoomController { get; }

        // Core
        public PartsGraphTreeBuilderViewModel Graph { get; }
        public ComponentSumController SumController { get; }
        private readonly ComponentSumControllerBinder _componentSumBinder;

        private readonly IVMPartsStore _store;

        public PartsGraphViewModel(IGlobalSelectedParts selectedParts, IMainWindowController mwc, PanAndZoomControllerViewModel pazc, PartsGraphTreeBuilderViewModel graph, IVMPartsStore store, IEventBus bus)
        {
            SelectedParts = selectedParts;
            MainWindowController = mwc;
            PanAndZoomController = pazc;
            Graph = graph;

            _store = store;

            var focusedPartChangedSubscription = bus.Subscribe<GlobalFocusedPartChangedEvent>(ev =>
                {
                    _store.TryGet(ev.FocusedPartUid, out var focused);
                    _store.TryGet(ev.PreviousSelectedPartUid, out var previous);
                    var focusedRecipe = focused?.GetRelatedRecipe();
                    var previousRecipe = previous?.GetRelatedRecipe();

                    if (focusedRecipe != previousRecipe)
                        CenterizePanPosition();
                });
            _subscriptions.Add(focusedPartChangedSubscription);

            SumController = new();
            _componentSumBinder = new ComponentSumControllerBinder(graph.ComponentLeafs, SumController);
        }

        public void Dispose()
        {
            foreach(var subscription in _subscriptions)
                subscription.Dispose();
        }

        // Commands
        [RelayCommand]
        public void CenterizePanPosition()
        {
            PanAndZoomController.CenterizePanPosition(Graph.RootNodeDefaultPosition);
        }

        [RelayCommand]
        public void ZoomIn()
        {
            PanAndZoomController.ZoomLevel *= 1.5;
        }

        [RelayCommand]
        public void ZoomOut()
        {
            PanAndZoomController.ZoomLevel /= 1.5;
        }
    }
}
