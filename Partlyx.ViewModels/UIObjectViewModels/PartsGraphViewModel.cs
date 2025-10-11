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
        private IEventBus _bus;
        public IGlobalSelectedParts SelectedParts { get; }

        // Subscriptions
        private readonly IDisposable _recipeChangedSubscription;
        private readonly IDisposable _recipeRemovedSubscription;

        private readonly IDisposable _componentAddedSubscription;
        private readonly IDisposable _componentRemovedSubscription;
        private readonly IDisposable _componentMovedSubscription;

        // Collections
        public ObservableCollection<TwoObjectsLineViewModel> Edges { get; } = new();
        public ObservableCollection<GraphTreeNodeViewModel> Nodes { get; } = new();
        private readonly Dictionary<Guid, GraphTreeNodeViewModel> _nodesDictionary = new();

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

        private void UpdateTree()
        {
            Nodes.Clear();
            _nodesDictionary.Clear();
            Edges.Clear();

            var selectedRecipe = SelectedParts.GetSingleRecipeOrNull();
            if (selectedRecipe == null) return;

            var mainNode = new RecipeGraphNodeViewModel(selectedRecipe);
            mainNode.XCentered = 1000;
            mainNode.YCentered = 1000;
            AddNode(mainNode);

            LoadChildComponentsFrom(selectedRecipe.Components, mainNode);

            void LoadChildComponentsFrom(IList<RecipeComponentItemViewModel> components, GraphTreeNodeViewModel parentNode)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];

                    var node = new ComponentGraphNodeViewModel(component);
                    AddNode(node);

                    parentNode.AddChild(node);

                    var edge = new TwoObjectsLineViewModel(parentNode, node);
                    AddEdge(edge);

                    if (component.SelectedRecipeComponents != null)
                        LoadChildComponentsFrom(component.SelectedRecipeComponents, node);
                }
            }

            mainNode.UpdateChildrenPositions();
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

        private void AddEdge(TwoObjectsLineViewModel edge)
        {
            Edges.Add(edge);
        }

        private void RemoveEdge(TwoObjectsLineViewModel edge)
        {
            Edges.Remove(edge);
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
    }
}
