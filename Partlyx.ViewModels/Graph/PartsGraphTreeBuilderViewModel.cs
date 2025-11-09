using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using static Partlyx.ViewModels.PartsViewModels.VMPartExtensions;
using System.Runtime.CompilerServices;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class PartsGraphTreeBuilderViewModel : GraphTreeBuilderViewModel
    {
        private List<IDisposable> _subscriptions = new();

        private IVMPartsStore _store;

        public PartsGraphTreeBuilderViewModel(IGlobalFocusedPart focusedPart, IEventBus bus, IVMPartsStore store)
        {
            _store = store;

            FocusedPart = focusedPart;

            var focusedPartChangedSubscription = bus.Subscribe<GlobalFocusedPartChangedEvent>(
                (ev) =>
                    {
                        _store.TryGet(ev.FocusedPartUid, out var focused);
                        _store.TryGet(ev.PreviousSelectedPartUid, out var previous);
                        var focusedRecipe = focused?.GetRelatedRecipe();
                        var previousRecipe = previous?.GetRelatedRecipe();

                        if (focusedRecipe != previousRecipe)
                            UpdateGraph();
                    });
            _subscriptions.Add(focusedPartChangedSubscription);
            var recipeRemovedSubscription = bus.Subscribe<RecipeVMRemovedFromStoreEvent>((ev) => UpdateGraph());
            _subscriptions.Add(recipeRemovedSubscription);

            _subscriptions.Add(bus.Subscribe<RecipeComponentCreatingCompletedVMEvent>((ev) => UpdateGraph()));
            _subscriptions.Add(bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>((ev) => UpdateGraph()));
            _subscriptions.Add(bus.Subscribe<RecipeComponentsMovingCompletedVMEvent>((ev) => UpdateGraph()));
        }

        public IGlobalFocusedPart FocusedPart { get; }

        public ObservableCollection<ComponentGraphNodeViewModel> ComponentLeafs { get; } = new();

        private void RebuildBranch(GraphTreeNodeViewModel rootNode)
        {
            DestroyBranch(rootNode, false);

            if (rootNode.Value is RecipeViewModel recipe)
                LoadChildComponentsFrom(recipe.Components, rootNode);
            else if (rootNode.Value is RecipeComponentViewModel component && component.SelectedRecipeComponents != null)
                LoadChildComponentsFrom(component.SelectedRecipeComponents, rootNode);
            else
                Trace.WriteLine("Cannot build branch for node with type: " + rootNode.Value?.GetType());

            void LoadChildComponentsFrom(IList<RecipeComponentViewModel> components, GraphTreeNodeViewModel parentNode)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];

                    var node = new ComponentGraphNodeViewModel(component);
                    AddNode(node);

                    parentNode.AddChild(node);

                    var edge = new TwoObjectsLineViewModel(parentNode, node);

                    if (component.SelectedRecipeComponents != null && component.SelectedRecipeComponents.Count > 0)
                        LoadChildComponentsFrom(component.SelectedRecipeComponents, node);
                    else
                        ComponentLeafs.Add(node);
                }
            }

            rootNode.BuildChildren();
        }

        private List<GraphTreeNodeViewModel> FindNodesByPartUid(Guid uid)
        {
            List<GraphTreeNodeViewModel> nodes = new();

            nodes = Nodes.Where(
                n =>
                {
                    var nodePartUid = (n.Value as IVMPart)?.Uid;
                    if (nodePartUid == null)
                        return false;
                    else
                        return nodePartUid.Equals(uid);
                })
                .ToList();

            return nodes;
        }

        public void UpdateGraph()
        {
            DestroyTree();

            // Getting the part to build the tree from it
            var selectedRecipe = FocusedPart.FocusedPart?.GetRelatedRecipe();
            if (selectedRecipe == null) return;

            // Creating the root node
            var mainNode = new RecipeGraphNodeViewModel(selectedRecipe);
            mainNode.XCentered = RootNodeDefaultPosition.X;
            mainNode.YCentered = RootNodeDefaultPosition.Y;
            AddNode(mainNode);
            RootNode = mainNode;

            // Building the tree
            RebuildBranch(mainNode);

            Edges = mainNode.GetBranchLinesMultiCollection();
            BuildEdgesFor(mainNode);
        }

        protected override void OnTreeDestroyed()
        {
            ComponentLeafs.Clear();
        }

        #region Unfinished optimization code
        //private void OnComponentAdded(RecipeComponentVMAddedToStoreEvent ev)
        //{
        //    var component = _store.RecipeComponents[ev.ComponentUid];
        //    if (component == null) return;

        //    AddComponent(component);
        //}

        //private void AddComponent(RecipeComponentViewModel component)
        //{
        //    var parentUid = component.LinkedParentRecipe?.Uid;
        //    if (parentUid == null) return;

        //    var componentParentNodes = FindNodesByPartUid((Guid)parentUid);

        //    foreach (var parentNode in componentParentNodes)
        //    {
        //        var componentNode = new ComponentGraphNodeViewModel(component);
        //        parentNode.AddChild(componentNode);

        //        AddNode(componentNode);

        //        var collections = componentNode.GetBranchLinesCollections();
        //        Edges.AddCollections(collections);
        //    }

        //    UpdateTreePositions();
        //}

        //private void OnComponentRemoved(RecipeComponentVMRemovedFromStoreEvent ev)
        //{
        //    var component = _store.RecipeComponents[ev.ComponentUid];
        //    if (component == null) return;

        //    RemoveComponentFromTree(component);
        //}

        //private void RemoveComponentFromTree(RecipeComponentViewModel component)
        //{
        //    var parentUid = component.LinkedParentRecipe?.Uid;
        //    if (parentUid == null) return;

        //    var componentParentNodes = FindNodesByPartUid((Guid)parentUid);

        //    foreach (var parentNode in componentParentNodes)
        //    {
        //        var componentNode = new ComponentGraphNodeViewModel(component);
        //        parentNode.RemoveChild(componentNode);

        //        RemoveNode(componentNode);

        //        var collections = componentNode.GetBranchLinesCollections();
        //        Edges.RemoveCollections(collections);
        //    }

        //    UpdateTreePositions();
        //}

        //private void OnComponentMoved(RecipeComponentMovedEvent ev)
        //{
        //    var component = _store.RecipeComponents[ev.ComponentUid];
        //    if (component == null) return;

        //    var previousComponentParentNodes = FindNodesByPartUid(ev.OldRecipeUid);
        //}

        //private GraphTreeNodeViewModel? GetComponentParentNode()
        //{
        //    var parentRecipeUid = component.LinkedParentRecipe?.Uid;
        //}
        #endregion
    }
}