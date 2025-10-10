using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsGraphViewModel : ObservableObject, IDisposable
    {
        // consts
        private const float StandardNodeDistanceX = 24;
        private const float StandardNodeDistanceY = 48;
        private const float StandardBranchDistanceX = StandardNodeDistanceX * 2;

        private IEventBus _bus;
        public IGlobalSelectedParts SelectedParts { get; }

        // Subscriptions
        private readonly IDisposable _recipeChangedSubscription;
        private readonly IDisposable _recipeRemovedSubscription;

        private readonly IDisposable _componentAddedSubscription;
        private readonly IDisposable _componentRemovedSubscription;
        private readonly IDisposable _componentMovedSubscription;

        // Collections
        public ObservableCollection<EdgeViewModel> Edges { get; } = new();
        public ObservableCollection<GraphNodeViewModel> Nodes { get; } = new();
        private readonly Dictionary<Guid, GraphNodeViewModel> _nodesDictionary = new();

        public PartsGraphViewModel(IGlobalSelectedParts selectedParts, IEventBus bus)
        {
            _bus = bus;
            SelectedParts = selectedParts;
            _recipeChangedSubscription = bus.Subscribe<GlobalSingleRecipeSelectedEvent>((ev) => UpdateTree());
            _recipeRemovedSubscription = bus.Subscribe<RecipeVMRemovedFromStoreEvent>((ev) => UpdateTree());

            _componentAddedSubscription = bus.Subscribe<RecipeComponentVMAddedToStoreEvent>((ev) => UpdateTree());
            _componentRemovedSubscription = bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>((ev) => UpdateTree());
            _componentMovedSubscription = bus.Subscribe<RecipeComponentMovedEvent>((ev) => UpdateTree());
        }

        private Vector2 GetChildrenSize(GraphNodeViewModel node)
        {
            if (node.ChildrenUids == null) return Vector2.Zero;

            int childrenCount = node.ChildrenUids.Count;
            float widthSum = 0;
            float height = 0;
            for (int i = 0; i < node.ChildrenUids.Count; i++)
            {
                var size = node.GetChildSize(i);

                widthSum += size.X;

                if (height < size.Y)
                    height = size.Y;
            }
                
            float width = widthSum + (childrenCount - 1) * StandardNodeDistanceX;
            return new Vector2(width, height);
        }

        private Vector2[] GetChildrenLocalPositions(GraphNodeViewModel node)
        {
            if (node.ChildrenUids == null) return [];

            Vector2 childrenSize = GetChildrenSize(node);
            float edgePointsDistanceX = childrenSize.X - childrenSize.X / node.ChildrenUids.Count;
            float commonOffsetX = -edgePointsDistanceX / 2;

            Vector2[] positions = new Vector2[node.ChildrenUids.Count];
            Vector2 lastPosition = default;
            float lastNodeWidth = default;
            for(int i = 0; i < node.ChildrenUids.Count; i++)
            {
                var size = node.GetChildSize(i);

                float positionX = lastPosition == default || lastNodeWidth == default 
                    ? commonOffsetX : lastPosition.X + lastNodeWidth + StandardNodeDistanceX;
                float positionY = node.Height + StandardNodeDistanceY;

                var position = new Vector2(positionX, positionY);
                positions[i] = position;
                lastPosition = position;
                lastNodeWidth = size.X;
            }
            return positions;
        }

        private void UpdateTree()
        {
            Nodes.Clear();
            _nodesDictionary.Clear();
            Edges.Clear();

            var selectedRecipe = SelectedParts.GetSingleRecipeOrNull();
            if (selectedRecipe == null) return;

            var mainNode = new RecipeGraphNodeViewModel(selectedRecipe);
            mainNode.XCentered = 5000;
            mainNode.YCentered = 5000;
            AddNode(mainNode);

            LoadChildComponentsFrom(selectedRecipe.Components, mainNode);

            void LoadChildComponentsFrom(IList<RecipeComponentItemViewModel> components, GraphNodeViewModel parentNode)
            {
                var childPositions = GetChildrenLocalPositions(parentNode);
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];

                    var node = new ComponentGraphNodeViewModel(component);
                    AddNode(node);

                    var localPos = childPositions[i];
                    var globalPosX = localPos.X + parentNode.XCentered;
                    var globalPosY = localPos.Y + parentNode.YCentered;

                    node.XCentered = globalPosX;
                    node.YCentered = globalPosY;

                    var edge = new EdgeViewModel(parentNode, node);
                    AddEdge(edge);

                    if (component.SelectedRecipeComponents != null)
                        LoadChildComponentsFrom(component.SelectedRecipeComponents, node);
                }
            }
        }

        private void AddNode(GraphNodeViewModel node)
        {
            Nodes.Add(node);
            _nodesDictionary.Add(node.Uid, node);
        }

        private void RemoveNode(GraphNodeViewModel node)
        {
            Nodes.Remove(node);
            _nodesDictionary.Remove(node.Uid);
        }

        private void AddEdge(EdgeViewModel edge)
        {
            Edges.Add(edge);
        }

        private void RemoveEdge(EdgeViewModel edge)
        {
            Edges.Remove(edge);
        }

        private GraphNodeViewModel? GetNodeByUid(Guid uid) => _nodesDictionary.GetValueOrDefault(uid);

        public void Dispose()
        {
            _recipeChangedSubscription.Dispose();
            _recipeRemovedSubscription.Dispose();

            _componentAddedSubscription.Dispose();
            _componentRemovedSubscription.Dispose();
            _componentMovedSubscription.Dispose();
        }
    }
}
