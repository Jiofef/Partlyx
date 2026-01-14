using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.GraphicsViewModels;
using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public sealed class PartsGraphBuilderViewModel : MSAGLGraphBuilderViewModel
    {
        private readonly IVMPartsStore _store;
        private readonly ThrottledInvoker _throttled;

        private readonly Dictionary<Guid, List<IVMPart>> _nodesByPartUid = new();
        public ReadOnlyDictionary<Guid, List<IVMPart>> NodesByPartUid { get; }

        public ObservableCollection<ComponentGraphNodeViewModel> ComponentLeafs { get; } = new();
        /// <summary>
        /// First arg is the old instance manager, the second is the new one
        /// </summary>
        public event Action<IPartsGraphInstanceManager?, IPartsGraphInstanceManager?> OnGraphBuilded = delegate { };
        public IGlobalFocusedElementContainer FocusedElement { get; }

        private IPartsGraphInstanceManager? _currentGraphManager;
        public IPartsGraphInstanceManager? CurrentGraphManager { get => _currentGraphManager; private set => SetProperty(ref _currentGraphManager, value); }

        private bool _isGraphCleared = true;
        public bool IsGraphCleared { get => _isGraphCleared; private set => SetProperty(ref _isGraphCleared, value); }

        public PartsGraphBuilderViewModel(
            IGlobalFocusedElementContainer focusedPart,
            IEventBus bus,
            IRoutedEventBus routedBus,
            IVMPartsStore store)
        {
            _store = store;
            FocusedElement = focusedPart;
            _throttled = new ThrottledInvoker(TimeSpan.FromMilliseconds(200));

            NodesByPartUid = new ReadOnlyDictionary<Guid, List<IVMPart>>(_nodesByPartUid);

            bus.Subscribe<GlobalFocusedElementChangedEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeVMRemovedFromStoreEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentCreatingCompletedVMEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentUpdatedViewModelEvent>(OnComponentUpdated);

            bus.Subscribe<RecipeComponentQuantityChangedEvent>(ev => OnComponentQuantityUpdated(ev.ComponentUid));
        }
        private void OnComponentUpdated(RecipeComponentUpdatedViewModelEvent ev)
        {
            if (NodesByPartUid.ContainsKey(ev.RecipeComponentUid)
                && (ev.ChangedProperties?.Contains(nameof(RecipeComponentViewModel.IsOutput)) ?? false))
            {
                UpdateGraph();
            }
        }
        private void OnComponentQuantityUpdated(Guid updatedComponentUid)
        {
            if (NodesByPartUid.ContainsKey(updatedComponentUid))
                CurrentGraphManager?.UpdateCosts();
        }
        private async Task<bool> BuildAsync()
        {
            ClearGraph();

            var focused = FocusedElement.Focused;
            if (focused == null)
                return false;

            var oldGraphManager = CurrentGraphManager;

            switch (focused.FocusableType)
            {
                case FocusableElementTypeEnum.RecipeHolder:
                    var recipe = focused.GetRelatedRecipe();
                    if (recipe == null)
                        return false;

                    if (CurrentGraphManager is not RecipeGraphInstanceManager rcg || rcg.RootRecipe != recipe)
                    {
                        if (CurrentGraphManager is IDisposable disposable)
                            disposable.Dispose();
                        CurrentGraphManager = new RecipeGraphInstanceManager(this, recipe, _store);
                    }

                    var recipeGraphManager = (RecipeGraphInstanceManager)CurrentGraphManager;
                    recipeGraphManager.BuildGraph();
                    break;

                case FocusableElementTypeEnum.ComponentPathHolder:
                    var path = focused as RecipeComponentPathItem;
                    if (path == null)
                        return false;

                    if (CurrentGraphManager is not ComponentPathGraphInstanceManager cpg || cpg.PathItem != path)
                    {
                        if (CurrentGraphManager is IDisposable disposable)
                            disposable.Dispose();
                        CurrentGraphManager = new ComponentPathGraphInstanceManager(this, path);
                    }

                    var pathGraphManager = (ComponentPathGraphInstanceManager)CurrentGraphManager;
                    pathGraphManager.BuildGraph();
                    break;
            }

            var newGraphManager = CurrentGraphManager;

            OnGraphBuilded?.Invoke(oldGraphManager, newGraphManager);
            IsGraphCleared = false;
            return true;
        }
        public void UpdateGraph() => _throttled.InvokeAsync(BuildAsync);

        protected override void OnNodeAdded(GraphNodeViewModel node)
        {
            RegisterNodeInDictionary(node);
        }

        protected override void OnNodeRemoved(GraphNodeViewModel node)
        {
            UnregisterNodeFromDictionary(node);
        }

        protected override void OnTreeCleared()
        {
            ComponentLeafs.Clear();
            _nodesByPartUid.Clear();
            IsGraphCleared = true;
        }

        private void RegisterNodeInDictionary(GraphNodeViewModel node)
        {
            IVMPart? part = node switch
            {
                ComponentGraphNodeViewModel c => c.Part,
                RecipeGraphNodeViewModel r => r.Part,
                ResourceGraphNodeViewModel res => res.Part,
                _ => null
            };

            if (part != null)
            {
                if (!_nodesByPartUid.TryGetValue(part.Uid, out var list))
                {
                    list = new List<IVMPart>();
                    _nodesByPartUid[part.Uid] = list;
                }
                list.Add(part);
            }
        }

        private void UnregisterNodeFromDictionary(GraphNodeViewModel node)
        {
            IVMPart? part = node switch
            {
                ComponentGraphNodeViewModel c => c.Part,
                RecipeGraphNodeViewModel r => r.Part,
                ResourceGraphNodeViewModel res => res.Part,
                _ => null
            };

            if (part != null && _nodesByPartUid.TryGetValue(part.Uid, out var list))
            {
                list.Remove(part);
                if (list.Count == 0)
                {
                    _nodesByPartUid.Remove(part.Uid);
                }
            }
        }
    }
}