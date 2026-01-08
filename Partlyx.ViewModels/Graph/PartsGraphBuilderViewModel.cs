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
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.Graph
{
    public sealed class PartsGraphBuilderViewModel : MSAGLGraphBuilderViewModel
    {
        private readonly IVMPartsStore _store;
        private readonly ThrottledInvoker _throttled;

        public ObservableCollection<ComponentGraphNodeViewModel> ComponentLeafs { get; } = new();
        public RelayCommand? OnGraphBuilded { get; set; }
        public IGlobalFocusedElementContainer FocusedElement { get; }

        public PartsGraphBuilderViewModel(
            IGlobalFocusedElementContainer focusedPart,
            IEventBus bus,
            IRoutedEventBus routedBus,
            IVMPartsStore store)
        {
            _store = store;
            FocusedElement = focusedPart;
            _throttled = new ThrottledInvoker(TimeSpan.FromMilliseconds(200));

            bus.Subscribe<GlobalFocusedElementChangedEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeVMRemovedFromStoreEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentCreatingCompletedVMEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>(_ => UpdateGraph());
        }

        public void UpdateGraph() => _throttled.InvokeAsync(BuildAsync);

        private async Task<bool> BuildAsync()
        {
            DestroyTree();
            ComponentLeafs.Clear();

            var focused = FocusedElement.Focused;
            if (focused == null)
                return false;

            switch (focused.FocusableType)
            {
                case FocusableElementTypeEnum.RecipeHolder:
                    var recipe = focused.GetRelatedRecipe();
                    if (recipe == null)
                        return false;

                    var recipeRoot = recipe.ToNode();
                    AddNode(recipeRoot);
                    RootNode = recipeRoot;

                    BuildRecipeNodesRecursively(recipeRoot, recipe, new HashSet<Guid> { recipe.Uid });

                    BuildEdges();

                    BuildLayout();
                    break;
                case FocusableElementTypeEnum.ComponentPathHolder:
                    var path = focused.GetRelatedRecipeComponentPath();
                    if (path == null || path.Nodes.Count == 0)
                        return false;

                    var steps = path.Nodes;

                    // Creating the root component node
                    var current = steps.First!;
                    var pathRoot = current.Value.ToNode();
                    AddNode(pathRoot);

                    // Creating the root component recipe node
                    var rootRecipe = current.Value.ParentRecipe;
                    if (rootRecipe == null)
                        return false;

                    // Creating the root component node siblings
                    var rootSiblings = current.Value.GetSiblings();
                    var previousLayerComponentNodes = new List<ComponentGraphNodeViewModel>() { pathRoot };
                    foreach (var sibling in rootSiblings)
                    {
                        var siblingNode = sibling.ToNode();
                        previousLayerComponentNodes.Add(siblingNode); 
                        AddNode(siblingNode);
                    }

                    // Building all the layers
                    var currentRecipe = rootRecipe;
                    var currentRecipeNode = rootRecipe.ToNode();
                    AddNode(currentRecipeNode);
                    while (current.Next != null)
                    {
                        // Binding parent component nodes to recipe
                        current = current.Next;
                        previousLayerComponentNodes.AddChildToAll(currentRecipeNode);

                        // Creating child components
                        var siblings = current.Value.GetWithSiblings();
                        var nextLayerComponentNodes = new List<ComponentGraphNodeViewModel>() { pathRoot };
                        foreach (var sibling in siblings)
                        {
                            var siblingNode = sibling.ToNode(currentRecipeNode);
                            nextLayerComponentNodes.Add(siblingNode);
                            AddNode(siblingNode);
                        }

                        // Binding the recipe to its child components
                        currentRecipeNode.AddChildren(nextLayerComponentNodes);

                        // Getting the next recipe and creating the node for it
                        currentRecipe = current.Value.ParentRecipe;
                        if (currentRecipe == null)
                            return false;

                        currentRecipeNode = currentRecipe.ToNode();

                        // Sending our children components collection to the next iteration
                        previousLayerComponentNodes = nextLayerComponentNodes;
                    }

                    break;
                default:
                    return false;
            }

            OnGraphBuilded?.Execute(null);
            return true;
        }

        private void BuildRecipeNodesRecursively(
            GraphNodeViewModel recipeNode,
            RecipeViewModel recipe,
            HashSet<Guid> path)
        {
            // Inputs → components → producer recipes
            foreach (var input in recipe.Inputs ?? Enumerable.Empty<RecipeComponentViewModel>())
            {
                var component = input.ToNode();
                AddNode(component);
                recipeNode.AddChild(component);

                var nextRecipe =
                    input.CurrentRecipe ??
                    (input.Resource?.LinkedDefaultRecipe?.Uid is Guid uid
                        ? _store.Recipes.GetValueOrDefault(uid)
                        : null);

                if (nextRecipe == null)
                {
                    ComponentLeafs.Add(component);
                    continue;
                }

                var nextRecipeNode = nextRecipe.ToNode();
                AddNode(nextRecipeNode);
                component.AddChild(nextRecipeNode);

                // Prevent infinite expansion, but allow visual recursion
                if (!path.Contains(nextRecipe.Uid))
                {
                    var nextPath = new HashSet<Guid>(path) { nextRecipe.Uid };
                    BuildRecipeNodesRecursively(nextRecipeNode, nextRecipe, nextPath);
                }
            }

            // Outputs (side products)
            foreach (var output in recipe.Outputs ?? Enumerable.Empty<RecipeComponentViewModel>())
            {
                var component = output.ToNode();
                AddNode(component);
                component.AddChild(recipeNode);
                ComponentLeafs.Add(component);
            }
        }

        protected override void OnTreeDestroyed()
        {
            ComponentLeafs.Clear();
        }
    }
}
