using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Interfaces;

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

        public PartsGraphViewModel(IGlobalSelectedParts selectedParts, IMainWindowController mwc, PanAndZoomControllerViewModel pazc, PartsGraphTreeBuilderViewModel graph, IEventBus bus)
        {
            SelectedParts = selectedParts;
            MainWindowController = mwc;
            PanAndZoomController = pazc;
            Graph = graph;

            var recipeChangedSubscription = bus.Subscribe<GlobalSingleRecipeSelectedEvent>(ev => CenterizePanPosition());
            _subscriptions.Add(recipeChangedSubscription);

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
