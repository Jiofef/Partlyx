using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.PartsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class RecipeGraphInstanceManager : IPartsGraphInstanceManager
    {
        private readonly IVMPartsStore _store;
        public PartsGraphBuilderViewModel ParentBuilder { get; }
        public RecipeViewModel RootRecipe { get; }

        public RecipeGraphInstanceManager(PartsGraphBuilderViewModel builder, RecipeViewModel rootRecipe, IVMPartsStore store)
        {
            _store = store;
            ParentBuilder = builder;
            RootRecipe = rootRecipe;
        }

        public void BuildGraph()
        {
            var b = ParentBuilder;
            b.ClearGraph();

            var rootRecipeNode = RootRecipe.ToNode();
            b.RootNode = rootRecipeNode;
            b.AddNode(rootRecipeNode);

            var visitedRecipes = new HashSet<Guid> { RootRecipe.Uid };

            // 1. Build Upward (Consumers of Root Recipe's Outputs)
            foreach (var output in RootRecipe.Outputs)
            {
                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(rootRecipeNode);

                if (output.ParentRecipe != null && !visitedRecipes.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visitedRecipes));
            }

            // 2. Build Downward (Producers of Root Recipe's Inputs)
            foreach (var input in RootRecipe.Inputs)
            {
                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                rootRecipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visitedRecipes.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visitedRecipes));
                else if (input.CurrentRecipe == null)
                    b.ComponentLeaves.Add(inputNode);
            }

            UpdateCosts();
            b.BuildEdges();
            b.BuildLayout();
        }

        private void BuildUpwardTree(ComponentGraphNodeViewModel bridgeNode, RecipeViewModel recipe, Guid? bridgeResourceUid, HashSet<Guid> visited)
        {
            var b = ParentBuilder;
            visited.Add(recipe.Uid);
            var recipeNode = recipe.ToNode();
            b.AddNode(recipeNode);
            recipeNode.AddChild(bridgeNode);

            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;
                var node = output.ToNode();
                b.AddNode(node);
                node.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(node, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visited));
            }

            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;
                var node = input.ToNode();
                b.AddNode(node);
                recipeNode.AddChild(node);

                if (input.CurrentRecipe != null && !visited.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(node, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visited));
                else if (input.CurrentRecipe == null)
                    b.ComponentLeaves.Add(node);
            }
        }

        private void BuildDownwardTree(ComponentGraphNodeViewModel bridgeNode, RecipeViewModel recipe, Guid? bridgeResourceUid, HashSet<Guid> visited)
        {
            var b = ParentBuilder;
            visited.Add(recipe.Uid);
            var recipeNode = recipe.ToNode();
            b.AddNode(recipeNode);
            bridgeNode.AddChild(recipeNode);

            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;
                var node = input.ToNode();
                b.AddNode(node);
                recipeNode.AddChild(node);

                if (input.CurrentRecipe != null && !visited.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(node, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visited));
                else if (input.CurrentRecipe == null)
                    b.ComponentLeaves.Add(node);
            }

            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;
                var node = output.ToNode();
                b.AddNode(node);
                node.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(node, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visited));
            }
        }

        public void UpdateCosts()
        {
            if (ParentBuilder.RootNode is not RecipeGraphNodeViewModel rootRecipeNode) return;
            ProcessRecipeNode(rootRecipeNode, 1.0, new HashSet<Guid>());
        }

        private void ProcessRecipeNode(RecipeGraphNodeViewModel recipeNode, double scale, HashSet<Guid> activeResourceBridges)
        {
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
                    if (producerNode != null && comp.CurrentRecipe.OutputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalOut) && totalOut != 0)
                    {
                        double nextScale = node.AbsCost / totalOut;
                        ProcessRecipeNode(producerNode, nextScale, activeResourceBridges);
                    }
                }
                // Upward to consumer
                else if (comp.ParentRecipe != null)
                {
                    var consumerNode = node.Parents.OfType<RecipeGraphNodeViewModel>().FirstOrDefault();
                    if (consumerNode != null && comp.ParentRecipe.InputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalIn) && totalIn != 0)
                    {
                        double nextScale = node.AbsCost / totalIn;
                        ProcessRecipeNode(consumerNode, nextScale, activeResourceBridges);
                    }
                }

                activeResourceBridges.Remove(comp.Resource.Uid);
            }
        }
    }
}