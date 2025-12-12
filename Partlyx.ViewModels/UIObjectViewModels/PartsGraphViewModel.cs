using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.GraphicsViewModels;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Interfaces;
using ReactiveUI;
using System.Diagnostics;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsGraphViewModel : ObservableObject, IDisposable
    {
        public IGlobalSelectedParts SelectedParts { get; }

        public IMainWindowController MainWindowController { get; }

        private List<IDisposable> _subscriptions = new();

        // Pan/zoom
        public PanAndZoomControllerViewModel PanAndZoomController { get; }
        public DynamicPanPositionController PanVelocityController { get; }

        // Core
        public PartsGraphTreeBuilderViewModel Graph { get; }
        public ComponentSumController SumController { get; }
        private readonly ComponentSumControllerBinder _componentSumBinder;

        private readonly IVMPartsStore _partsStore;
        private readonly IImagesStoreViewModel _imagesStore;

        public PartsGraphViewModel(IGlobalSelectedParts selectedParts, IMainWindowController mwc, PanAndZoomControllerViewModel pazc, PartsGraphTreeBuilderViewModel graph, IVMPartsStore store, 
            IImagesStoreViewModel imagesStore, IEventBus bus, ITimerService timerService)
        {
            SelectedParts = selectedParts;
            MainWindowController = mwc;
            PanAndZoomController = pazc;
            Graph = graph;

            PanVelocityController = new DynamicPanPositionController(PanAndZoomController, timerService);

            _partsStore = store;
            _imagesStore = imagesStore;

            var focusedPartChangedSubscription = bus.Subscribe<GlobalFocusedPartChangedEvent>(ev =>
                {
                    _partsStore.TryGet(ev.FocusedPartUid, out var focused);
                    _partsStore.TryGet(ev.PreviousSelectedPartUid, out var previous);
                    var focusedRecipe = focused?.GetRelatedRecipe();
                    var previousRecipe = previous?.GetRelatedRecipe();

                    if (focusedRecipe != previousRecipe)
                        CenterizePanPosition();
                });
            _subscriptions.Add(focusedPartChangedSubscription);

            SumController = new();
            _componentSumBinder = new ComponentSumControllerBinder(graph.ComponentLeafs, SumController);

            Graph.OnGraphBuilded = new(async () => 
            {
                // We find all the unique images among the displayed nodes, and send a request to download the full version of the image instead of the compressed one.

                List<Guid> nodeImagesUids = new();
                HashSet<Guid> nodeImagesUidsHashed = new();
                foreach (var node in Graph.Nodes)
                {
                    if (node.Value is not IVMPart part) continue;
                    if (part.Icon?.Content is not ImageViewModel image) continue;
                    if (nodeImagesUidsHashed.Contains(image.Uid)) continue;
                    
                    nodeImagesUidsHashed.Add(image.Uid);
                    nodeImagesUids.Add(image.Uid);
                }

                if (nodeImagesUids.Count <= 0) return;

                await imagesStore.LoadFullImages(nodeImagesUids.ToArray());
            });
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
