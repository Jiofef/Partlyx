using DynamicData;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class RecipeGraphInstanceManager : PartlyxObservable, IPartsGraphInstanceManager
    {
        private readonly IVMPartsStore _store;
        public PartsGraphBuilderViewModel ParentBuilder { get; }
        public RecipeViewModel RootRecipe { get; }

        public RecipeGraphInstanceManager(PartsGraphBuilderViewModel builder, RecipeViewModel rootRecipe, IVMPartsStore store)
        {
            _store = store;
            ParentBuilder = builder;
            RootRecipe = rootRecipe;

            Disposables.Add(ParentBuilder.WhenAnyValue(b => b.RecipeMultiplier).Subscribe(_ => UpdateCosts()));
        }

        public void BuildGraph()
        {
            var b = ParentBuilder;
            b.ClearGraph();

            var rootRecipeNode = RootRecipe.ToNode();
            b.RootNode = rootRecipeNode;
            b.AddNode(rootRecipeNode);

            var visitedRecipes = new Dictionary<Guid, Guid> { { RootRecipe.Uid, rootRecipeNode.Uid } };

            // 1. Build Upward (Consumers of Root Recipe's Outputs)
            foreach (var output in RootRecipe.Outputs)
            {
                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(rootRecipeNode);

                if (output.ParentRecipe != null && !visitedRecipes.ContainsKey(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, visitedRecipes);
                else
                    b.ComponentLeaves.Add(outputNode);
            }

            // 2. Build Downward (Producers of Root Recipe's Inputs)
            foreach (var input in RootRecipe.Inputs)
            {
                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                rootRecipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visitedRecipes.ContainsKey(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, visitedRecipes);
                else
                    b.ComponentLeaves.Add(inputNode);
            }

            UpdateCosts();
            b.BuildEdges();
            b.BuildLayout();
        }

        // The dictionary values for the keys from the `visited` dictionary are not currently in use.
        // They are intended for the smart processing of cyclic recipes and require some modifications to the graph-building logic for implementation.
        private void BuildUpwardTree(ComponentGraphNodeViewModel bridgeNode, RecipeViewModel recipe, Guid? bridgeResourceUid, Dictionary<Guid, Guid> visited)
        {
            var b = ParentBuilder;
            var recipeNode = recipe.ToNode();
            b.AddNode(recipeNode);
            recipeNode.AddChild(bridgeNode);
            visited[recipe.Uid] = recipeNode.Uid;

            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;
                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.ContainsKey(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, visited);
                else
                    b.ComponentLeaves.Add(outputNode);
            }

            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;
                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                recipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visited.ContainsKey(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, visited);
                else
                    b.ComponentLeaves.Add(inputNode);
            }

            visited.Remove(recipe.Uid);
        }

        private void BuildDownwardTree(ComponentGraphNodeViewModel bridgeNode, RecipeViewModel recipe, Guid? bridgeResourceUid, Dictionary<Guid, Guid> visited)
        {
            var b = ParentBuilder;
            var recipeNode = recipe.ToNode();
            b.AddNode(recipeNode);
            bridgeNode.AddChild(recipeNode);
            visited[recipe.Uid] = recipeNode.Uid;

            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;
                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                recipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visited.ContainsKey(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, visited);
                else
                {
                    // If recipe is already visited, connect component to existing recipe node
                    b.ComponentLeaves.Add(inputNode);
                }
            }

            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;
                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.ContainsKey(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, visited);
                else
                    b.ComponentLeaves.Add(outputNode);
            }

            visited.Remove(recipe.Uid);
        }

        public void UpdateCosts()
        {
            if (ParentBuilder.RootNode is not RecipeGraphNodeViewModel rootRecipeNode) return;
            ProcessRecipeNodeCosts(rootRecipeNode, ParentBuilder.RecipeMultiplier, new(), new());
        }

        private void ProcessRecipeNodeCosts(RecipeGraphNodeViewModel recipeNode, double scale, HashSet<Guid> activeResourceBridges, HashSet<RecipeGraphNodeViewModel> processedNodes)
        {
            processedNodes.Add(recipeNode);

            var recipe = recipeNode.Part;
            if (recipe == null) return;

            var componentNodes = recipeNode.Parents.OfType<ComponentGraphNodeViewModel>()
                .Concat(recipeNode.Children.OfType<ComponentGraphNodeViewModel>());

            // First pass: update costs for all components in the current recipe
            foreach (var node in componentNodes)
            {
                var comp = node.Part;
                if (comp == null || activeResourceBridges.Contains(comp.Resource.Uid)) continue;

                node.AbsCost = comp.Quantity * scale;
                node.IsOutput = !node.Parents.Any();
            }

            // Second pass: propagate scale to connected recipes
            foreach (var node in componentNodes)
            {
                var comp = node.Part;
                if (comp == null || activeResourceBridges.Contains(comp.Resource.Uid)) continue;

                activeResourceBridges.Add(comp.Resource.Uid);

                // Downward to producer
                if (comp.CurrentRecipe != null)
                {
                    var producerNode = node.Children.OfType<RecipeGraphNodeViewModel>().FirstOrDefault();
                    if (producerNode != null && !processedNodes.Contains(producerNode) &&
                        comp.CurrentRecipe.OutputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalOut) && totalOut != 0)
                    {
                        double nextScale = node.AbsCost / totalOut;
                        ProcessRecipeNodeCosts(producerNode, nextScale, activeResourceBridges, processedNodes);
                    }
                }
                // Upward to consumer
                else if (comp.ParentRecipe != null)
                {
                    var consumerNode = node.Parents.OfType<RecipeGraphNodeViewModel>().FirstOrDefault();
                    if (consumerNode != null && !processedNodes.Contains(consumerNode) &&
                        comp.ParentRecipe.InputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalIn) && totalIn != 0)
                    {
                        double nextScale = node.AbsCost / totalIn;
                        ProcessRecipeNodeCosts(consumerNode, nextScale, activeResourceBridges, processedNodes);
                    }
                }

                activeResourceBridges.Remove(comp.Resource.Uid);
            }
        }
    }
}