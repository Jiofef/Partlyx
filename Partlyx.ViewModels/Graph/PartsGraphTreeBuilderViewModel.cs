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
using Partlyx.UI.Avalonia.Helpers;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceImplementations;

namespace Partlyx.ViewModels.Graph
{
    public enum GraphType { RecipeGraph, PathGraph }

    public class PartsGraphTreeBuilderViewModel : GraphTreeBuilderViewModel
    {
        private List<IDisposable> _subscriptions = new();

        private IVMPartsStore _store;
        private readonly GraphLayoutEngine _layoutEngine = new();

        // Graph display options
        private bool _showIntermediateRecipesForRecipeGraph = false;
        public bool ShowIntermediateRecipesForRecipeGraph
        {
            get => _showIntermediateRecipesForRecipeGraph;
            set
            {
                if (_showIntermediateRecipesForRecipeGraph != value)
                {
                    _showIntermediateRecipesForRecipeGraph = value;
                    UpdateGraph();
                }
            }
        }

        private bool _showRecipesBetweenComponentsForPathGraph = true;
        public bool ShowRecipesBetweenComponentsForPathGraph
        {
            get => _showRecipesBetweenComponentsForPathGraph;
            set
            {
                if (_showRecipesBetweenComponentsForPathGraph != value)
                {
                    _showRecipesBetweenComponentsForPathGraph = value;
                    UpdateGraph();
                }
            }
        }

        public RelayCommand? OnGraphBuilded { get; set; }

        public PartsGraphTreeBuilderViewModel(IGlobalFocusedElementContainer focusedPart, IEventBus bus, IRoutedEventBus routedBus, IVMPartsStore store)
        {
            _store = store;

            FocusedPart = focusedPart;

            _updateGraphTrottledInvoker = new(TimeSpan.FromMilliseconds(200));

            var focusedPartChangedSubscription = bus.Subscribe<GlobalFocusedElementChangedEvent>(
                (ev) =>
                    {
                        var focusedRecipe = ev.NewFocused?.GetRelatedRecipe();
                        var previousRecipe = ev.PreviousFocused?.GetRelatedRecipe();

                        if (focusedRecipe != previousRecipe)
                            UpdateGraph();
                    });
            _subscriptions.Add(focusedPartChangedSubscription);
            var recipeRemovedSubscription = bus.Subscribe<RecipeVMRemovedFromStoreEvent>((ev) => UpdateGraph());
            _subscriptions.Add(recipeRemovedSubscription);

            _subscriptions.Add(bus.Subscribe<RecipeComponentCreatingCompletedVMEvent>((ev) => UpdateGraph()));
            _subscriptions.Add(bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>((ev) => UpdateGraph()));
            _subscriptions.Add(bus.Subscribe<RecipeComponentsMovingCompletedVMEvent>((ev) => UpdateGraph()));
            _subscriptions.Add(routedBus.Subscribe<ResourceUpdatedViewModelEvent>("DefaultRecipeUid", (ev) => UpdateGraph()));
            _subscriptions.Add(routedBus.Subscribe<RecipeComponentUpdatedViewModelEvent>("SelectedRecipeUid", ev => UpdateGraph()));
        }

        public IGlobalFocusedElementContainer FocusedPart { get; }

        public ObservableCollection<ComponentGraphNodeViewModel> ComponentLeafs { get; } = new();

        private void RebuildBranch(GraphTreeNodeViewModel rootNode)
        {
            DestroyBranch(rootNode, false);

            HashSet<ResourceViewModel> parentResources = new();

            if (rootNode.Value is RecipeViewModel recipe)
            {
                parentResources.Add(recipe.LinkedParentResource!.Value!);
                LoadChildComponentsFrom(recipe.Inputs, rootNode);
            }
            else if (rootNode.Value is RecipeComponentViewModel component && component.SelectedRecipeComponents != null)
            {
                parentResources.Add(component.Resource);
                LoadChildComponentsFrom(component.SelectedRecipeComponents, rootNode);
            }
            else
                Trace.WriteLine("Cannot build branch for node with type: " + rootNode.Value?.GetType());

            void LoadChildComponentsFrom(IList<RecipeComponentViewModel> components, GraphTreeNodeViewModel parentNode)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    var componentResource = component.Resource;

                    var node = new ComponentGraphNodeViewModel(component);
                    AddNode(node);

                    parentNode.AddChild(node);

                    var edge = new TwoObjectsLineViewModel(parentNode, node);

                    if (component.SelectedRecipeComponents != null && component.SelectedRecipeComponents.Count > 0 && !parentResources.Contains(componentResource))
                    {
                        parentResources.Add(componentResource);
                        LoadChildComponentsFrom(component.SelectedRecipeComponents, node);
                    }
                    else
                        ComponentLeafs.Add(node);

                    parentResources.Remove(componentResource);
                }
            }
        }

        private ThrottledInvoker _updateGraphTrottledInvoker;
        public void UpdateGraph()
        {
            _updateGraphTrottledInvoker.InvokeAsync(UpdateGraphPrivate);
        }
        public static GraphType GetGraphType(IFocusable? focusable)
        {
            if (focusable == null) return GraphType.RecipeGraph;

            return focusable.FocusableType switch
            {
                FocusableElementTypeEnum.RecipeHolder => GraphType.RecipeGraph,
                FocusableElementTypeEnum.ComponentPathHolder => GraphType.PathGraph,
                _ => GraphType.RecipeGraph
            };
        }

        private Task UpdateGraphPrivate()
        {
            DestroyTree();

            var focused = FocusedPart.Focused;
            var graphType = GetGraphType(focused);

            if (graphType == GraphType.RecipeGraph)
            {
                return BuildRecipeGraph(focused?.GetRelatedRecipe());
            }
            else // PathGraph
            {
                return BuildPathGraph(focused as RecipeComponentPath);
            }
        }

        private Task BuildRecipeGraph(RecipeViewModel? recipe)
        {
            if (recipe == null) return Task.CompletedTask;

            // Creating the recipe node
            var recipeNode = new RecipeGraphNodeViewModel(recipe);
            recipeNode.XCentered = RootNodeDefaultPosition.X;
            recipeNode.YCentered = RootNodeDefaultPosition.Y;
            AddNode(recipeNode);
            RootNode = recipeNode;

            // Create nodes for outputs and inputs
            var outputNodes = new List<GraphTreeNodeViewModel>();
            var inputNodes = new List<GraphTreeNodeViewModel>();

            foreach (var output in recipe.Outputs)
            {
                var node = new ComponentGraphNodeViewModel(output);
                outputNodes.Add(node);
                AddNode(node);
                recipeNode.AddChild(node); // Outputs as children for now, but will be positioned above
            }

            foreach (var input in recipe.Inputs)
            {
                var node = new ComponentGraphNodeViewModel(input);
                inputNodes.Add(node);
                AddNode(node);
                recipeNode.AddChild(node); // Inputs as children

                // Build further branches for inputs
                if (input.SelectedRecipeComponents != null && input.SelectedRecipeComponents.Count > 0)
                {
                    RebuildBranch(node);
                }
                else
                {
                    ComponentLeafs.Add(node);
                }
            }

            // Apply special layout for recipe graph
            _layoutEngine.LayoutAsRecipeGraph(recipeNode, outputNodes, inputNodes);

            Edges = recipeNode.GetBranchLinesMultiCollection();
            BuildEdgesFor(recipeNode);

            if (OnGraphBuilded != null)
            {
                OnGraphBuilded.Execute(null);
            }

            return Task.CompletedTask;
        }

        private Task BuildPathGraph(RecipeComponentPath? path)
        {
            if (path == null || path.Nodes.Count == 0) return Task.CompletedTask;

            // Collect all components and their parent recipes
            var components = new HashSet<RecipeComponentViewModel>();
            var recipes = new HashSet<RecipeViewModel>();

            foreach (var component in path.Nodes)
            {
                components.Add(component);
                if (component.LinkedParentRecipe?.Value is RecipeViewModel recipe)
                {
                    recipes.Add(recipe);
                }
            }

            // Create nodes
            var componentNodes = new Dictionary<RecipeComponentViewModel, ComponentGraphNodeViewModel>();
            var recipeNodes = new Dictionary<RecipeViewModel, RecipeGraphNodeViewModel>();

            foreach (var component in components)
            {
                var node = new ComponentGraphNodeViewModel(component);
                componentNodes[component] = node;
                AddNode(node);
            }

            foreach (var recipe in recipes)
            {
                var node = new RecipeGraphNodeViewModel(recipe);
                recipeNodes[recipe] = node;
                AddNode(node);
            }

            // Create connections
            foreach (var component in components)
            {
                if (component.LinkedParentRecipe?.Value is RecipeViewModel recipe && recipeNodes.TryGetValue(recipe, out var recipeNode) && componentNodes.TryGetValue(component, out var componentNode))
                {
                    // Connect component to its recipe
                    recipeNode.AddChild(componentNode);
                }
            }

            // If showing recipes between components, add recipe -> next component connections
            if (ShowRecipesBetweenComponentsForPathGraph)
            {
                var componentList = path.Nodes.ToList();
                for (int i = 0; i < componentList.Count - 1; i++)
                {
                    var currentComponent = componentList[i];
                    var nextComponent = componentList[i + 1];

                    if (currentComponent.LinkedParentRecipe?.Value is RecipeViewModel recipe &&
                        recipeNodes.TryGetValue(recipe, out var recipeNode) &&
                        componentNodes.TryGetValue(nextComponent, out var nextComponentNode))
                    {
                        // If next component is in recipe's outputs or inputs
                        if (recipe.Inputs.Contains(nextComponent) || recipe.Outputs.Contains(nextComponent))
                        {
                            recipeNode.AddChild(nextComponentNode);
                        }
                    }
                }
            }

            // Set root and layout
            var firstComponent = path.Nodes.First.Value;
            if (componentNodes.TryGetValue(firstComponent, out var rootNode))
            {
                RootNode = rootNode;
                rootNode.XCentered = RootNodeDefaultPosition.X;
                rootNode.YCentered = RootNodeDefaultPosition.Y;

                // Apply layout
                _layoutEngine.LayoutAsTree(rootNode);

                Edges = rootNode.GetBranchLinesMultiCollection();
                BuildEdgesFor(rootNode);
            }

            if (OnGraphBuilded != null)
            {
                OnGraphBuilded.Execute(null);
            }

            return Task.CompletedTask;
        }
        protected override void OnTreeDestroyed()
        {
            ComponentLeafs.ClearAndDispose();
        }
    }
}
