using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.GraphicsViewModels;
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
        public IGlobalFocusedElementContainer FocusedPart { get; }

        public PartsGraphBuilderViewModel(
            IGlobalFocusedElementContainer focusedPart,
            IEventBus bus,
            IRoutedEventBus routedBus,
            IVMPartsStore store)
        {
            _store = store;
            FocusedPart = focusedPart;
            _throttled = new ThrottledInvoker(TimeSpan.FromMilliseconds(200));

            bus.Subscribe<GlobalFocusedElementChangedEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeVMRemovedFromStoreEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentCreatingCompletedVMEvent>(_ => UpdateGraph());
            bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>(_ => UpdateGraph());
        }

        public void UpdateGraph() => _throttled.InvokeAsync(BuildAsync);

        private async Task BuildAsync()
        {
            DestroyTree();
            ComponentLeafs.Clear();

            var recipe = FocusedPart.Focused?.GetRelatedRecipe();
            if (recipe == null)
                return;

            var root = new RecipeGraphNodeViewModel(recipe);
            AddNode(root);
            RootNode = root;

            BuildRecursively(root, recipe, new HashSet<Guid> { recipe.Uid });

            BuildEdges();

            BuildLayout();

            OnGraphBuilded?.Execute(null);
        }

        /// <summary>
        /// Builds DAG structure only. No positioning logic allowed here.
        /// </summary>
        private void BuildRecursively(
            GraphNodeViewModel recipeNode,
            RecipeViewModel recipe,
            HashSet<Guid> path)
        {
            // Inputs → components → producer recipes
            foreach (var input in recipe.Inputs ?? Enumerable.Empty<RecipeComponentViewModel>())
            {
                var component = new ComponentGraphNodeViewModel(input);
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

                var nextRecipeNode = new RecipeGraphNodeViewModel(nextRecipe);
                AddNode(nextRecipeNode);
                component.AddChild(nextRecipeNode);

                // Prevent infinite expansion, but allow visual recursion
                if (!path.Contains(nextRecipe.Uid))
                {
                    var nextPath = new HashSet<Guid>(path) { nextRecipe.Uid };
                    BuildRecursively(nextRecipeNode, nextRecipe, nextPath);
                }
            }

            // Outputs (side products)
            foreach (var output in recipe.Outputs ?? Enumerable.Empty<RecipeComponentViewModel>())
            {
                var component = new ComponentGraphNodeViewModel(output);
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
