using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.GraphicsViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsGraphViewModel : ObservableObject, IDisposable
    {
        public IGlobalSelectedParts SelectedParts { get; }

        // Subscriptions
        private readonly IDisposable _recipeChangedSubscription;
        private readonly IDisposable _recipeRemovedSubscription;

        private readonly IDisposable _componentAddedSubscription;
        private readonly IDisposable _componentRemovedSubscription;
        private readonly IDisposable _componentMovedSubscription;

        // Collections
        private ObservableMultiCollection<FromToLineViewModel> _edges = new();

        public ObservableMultiCollection<FromToLineViewModel> Edges { get => _edges; private set => SetProperty(ref _edges, value); } 

        public ObservableCollection<GraphTreeNodeViewModel> Nodes { get; } = new();

        private readonly Dictionary<Guid, GraphTreeNodeViewModel> _nodesDictionary = new();

        // Pan/zoom
        public PanAndZoomControllerViewModel PanAndZoomController { get; }

        public Point RootNodeDefaultPosition { get; } = new Point(0, 0);

        public PartsGraphViewModel(IGlobalSelectedParts selectedParts, IEventBus bus, PanAndZoomControllerViewModel pazc)
        {
            SelectedParts = selectedParts;
            PanAndZoomController = pazc;

            _recipeChangedSubscription = bus.Subscribe<GlobalSingleRecipeSelectedEvent>((ev) => OnSelectedTreeChanged());
            _recipeRemovedSubscription = bus.Subscribe<RecipeVMRemovedFromStoreEvent>((ev) => UpdateGraph());

            _componentAddedSubscription = bus.Subscribe<RecipeComponentVMAddedToStoreEvent>((ev) => UpdateGraph());
            _componentRemovedSubscription = bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>((ev) => UpdateGraph());
            _componentMovedSubscription = bus.Subscribe<RecipeComponentMovedEvent>((ev) => UpdateGraph());
        }

        private void OnSelectedTreeChanged()
        {
            CenterizePanPosition();
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            Nodes.Clear();
            _nodesDictionary.Clear();
            Edges.ClearCollections();

            var selectedRecipe = SelectedParts.GetSingleRecipeOrNull();
            if (selectedRecipe == null) return;

            var mainNode = new RecipeGraphNodeViewModel(selectedRecipe);
            mainNode.XCentered = RootNodeDefaultPosition.X;
            mainNode.YCentered = RootNodeDefaultPosition.Y;
            AddNode(mainNode);

            LoadChildComponentsFrom(selectedRecipe.Components, mainNode);

            void LoadChildComponentsFrom(IList<RecipeComponentViewModel> components, GraphTreeNodeViewModel parentNode)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];

                    var node = new ComponentGraphNodeViewModel(component);
                    AddNode(node);

                    parentNode.AddChild(node);

                    var edge = new TwoObjectsLineViewModel(parentNode, node);

                    if (component.SelectedRecipeComponents != null)
                        LoadChildComponentsFrom(component.SelectedRecipeComponents, node);
                }
            }
            
            mainNode.UpdateChildrenPositions();

            Edges = mainNode.GetBranchLinesMultiCollection();

            BuildEdgesFor(mainNode);
        }

        private Vector2 GetChildPositionFromParentPosition(GraphTreeNodeViewModel child, Vector2 parentPosition) 
            => new Vector2(parentPosition.X + child.XLocal, parentPosition.Y + child.YLocal); 

        private void BuildEdgesFor(GraphTreeNodeViewModel node)
        {
            var nodePosition = node.GetPositionCentered();
            BuildEdgesFor(node, nodePosition);
        }
        // To avoid unnecesarry global position chain calculations
        private void BuildEdgesFor(GraphTreeNodeViewModel node, Vector2 nodePositionCentered)
        {
            node.ConnectedLines.Clear();
            if (node.Children.Count == 0) return;

            if (node.Children.Count == 1)
            {
                var child = (GraphTreeNodeViewModel)node.Children.First();
                var childPosition = GetChildPositionFromParentPosition(child, nodePositionCentered);

                var line = new FromToLineViewModel(nodePositionCentered, childPosition);
                node.ConnectedLines.Add(line);

                BuildEdgesFor(child, childPosition);
            }
            else // if children is two or more
            {
                var firstChildPosition = GetChildPositionFromParentPosition((GraphTreeNodeViewModel)node.Children.First(), nodePositionCentered);
                var lastChildPosition = GetChildPositionFromParentPosition((GraphTreeNodeViewModel)node.Children.Last(), nodePositionCentered);
                float middleHorizontalLineYOffset = nodePositionCentered.Y + node.Height / 2 + node.SingleChildrenDistanceY / 2;

                var lineToHorizontalLine = new FromToLineViewModel(
                    nodePositionCentered,
                    new Vector2(nodePositionCentered.X, middleHorizontalLineYOffset));
                node.ConnectedLines.Add(lineToHorizontalLine);

                var horizontalLine = new FromToLineViewModel(
                    new Vector2(firstChildPosition.X, middleHorizontalLineYOffset),
                    new Vector2(lastChildPosition.X, middleHorizontalLineYOffset));
                node.ConnectedLines.Add(horizontalLine);

                foreach (GraphTreeNodeViewModel child in node.Children)
                {
                    var childPosition = GetChildPositionFromParentPosition(child, nodePositionCentered);

                    var lineFromHorizontalLine = new FromToLineViewModel(
                        new Vector2(childPosition.X, middleHorizontalLineYOffset),
                        childPosition);
                    node.ConnectedLines.Add(lineFromHorizontalLine);

                    BuildEdgesFor(child, childPosition);
                }
            }
        }

        private void AddNode(GraphTreeNodeViewModel node)
        {
            Nodes.Add(node);
            _nodesDictionary.Add(node.Uid, node);
        }

        private void RemoveNode(GraphTreeNodeViewModel node)
        {
            Nodes.Remove(node);
            _nodesDictionary.Remove(node.Uid);
        }

        private GraphTreeNodeViewModel? GetNodeByUid(Guid uid) => _nodesDictionary.GetValueOrDefault(uid);

        public void Dispose()
        {
            _recipeChangedSubscription.Dispose();
            _recipeRemovedSubscription.Dispose();

            _componentAddedSubscription.Dispose();
            _componentRemovedSubscription.Dispose();
            _componentMovedSubscription.Dispose();
        }

        // Commands
        [RelayCommand]
        public void CenterizePanPosition()
        {
            PanAndZoomController.CenterizePanPosition(RootNodeDefaultPosition);
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
