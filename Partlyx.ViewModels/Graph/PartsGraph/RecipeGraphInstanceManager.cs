using DynamicData;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
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

            // Root Recipe Node
            var rootRecipeNode = RootRecipe.ToNode();
            b.RootNode = rootRecipeNode;
            b.AddNode(rootRecipeNode);

            var visitedRecipes = new HashSet<Guid> { RootRecipe.Uid };

            // Start Upward building from Root Recipe Outputs
            foreach (var output in RootRecipe.Outputs)
            {
                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(rootRecipeNode); // Recipe -> Output

                if (output.ParentRecipe != null && !visitedRecipes.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visitedRecipes));
                else
                    b.ComponentLeaves.Add(outputNode);
            }

            // Start Downward building from Root Recipe Inputs
            foreach (var input in RootRecipe.Inputs)
            {
                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                rootRecipeNode.AddChild(inputNode); // Recipe -> Input

                if (input.CurrentRecipe != null && !visitedRecipes.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visitedRecipes));
                else
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

            // Link the bridge node. In this recipe, it acts as an INPUT.
            recipeNode.AddChild(bridgeNode);

            // Process Outputs of this consumer recipe
            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;

                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visited));
                else
                    b.ComponentLeaves.Add(outputNode);
            }

            // Process other Inputs
            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;

                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                recipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visited.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visited));
                else
                    b.ComponentLeaves.Add(inputNode);
            }
        }

        private void BuildDownwardTree(ComponentGraphNodeViewModel bridgeNode, RecipeViewModel recipe, Guid? bridgeResourceUid, HashSet<Guid> visited)
        {
            var b = ParentBuilder;
            visited.Add(recipe.Uid);
            var recipeNode = recipe.ToNode();
            b.AddNode(recipeNode);

            // Link the bridge node. In this recipe, it acts as an OUTPUT.
            bridgeNode.AddChild(recipeNode);

            // Process Inputs
            foreach (var input in recipe.Inputs)
            {
                if (input.Resource?.Uid == bridgeResourceUid) continue;

                var inputNode = input.ToNode();
                b.AddNode(inputNode);
                recipeNode.AddChild(inputNode);

                if (input.CurrentRecipe != null && !visited.Contains(input.CurrentRecipe.Uid))
                    BuildDownwardTree(inputNode, input.CurrentRecipe, input.Resource?.Uid, new HashSet<Guid>(visited));
                else
                    b.ComponentLeaves.Add(inputNode);
            }

            // Process other Outputs
            foreach (var output in recipe.Outputs)
            {
                if (output.Resource?.Uid == bridgeResourceUid) continue;

                var outputNode = output.ToNode();
                b.AddNode(outputNode);
                outputNode.AddChild(recipeNode);

                if (output.ParentRecipe != null && !visited.Contains(output.ParentRecipe.Uid))
                    BuildUpwardTree(outputNode, output.ParentRecipe, output.Resource?.Uid, new HashSet<Guid>(visited));
                else
                    b.ComponentLeaves.Add(outputNode);
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

            // 1. Process Parents (Outputs of current recipe node)
            foreach (var parentNode in recipeNode.Parents.OfType<ComponentGraphNodeViewModel>())
            {
                var comp = parentNode.Part;
                if (comp == null || activeResourceBridges.Contains(comp.Resource.Uid)) continue;

                parentNode.AbsCost = comp.Quantity * scale;
                parentNode.IsOutput = !parentNode.Parents.Any();

                // Move to consumer recipe (Upward)
                if (comp.ParentRecipe != null)
                {
                    var consumerNode = parentNode.Parents.OfType<RecipeGraphNodeViewModel>().FirstOrDefault();
                    if (consumerNode != null)
                    {
                        activeResourceBridges.Add(comp.Resource.Uid);
                        if (comp.ParentRecipe.InputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalIn) && totalIn != 0)
                        {
                            double nextScale = parentNode.AbsCost / totalIn;
                            ProcessRecipeNode(consumerNode, nextScale, activeResourceBridges);
                        }
                        activeResourceBridges.Remove(comp.Resource.Uid);
                    }
                }
            }

            // 2. Process Children (Inputs of current recipe node)
            foreach (var childNode in recipeNode.Children.OfType<ComponentGraphNodeViewModel>())
            {
                var comp = childNode.Part;
                if (comp == null || activeResourceBridges.Contains(comp.Resource.Uid)) continue;

                childNode.AbsCost = comp.Quantity * scale;
                childNode.IsOutput = !childNode.Parents.Any();

                // Move to producer recipe (Downward)
                if (comp.CurrentRecipe != null)
                {
                    var producerNode = childNode.Children.OfType<RecipeGraphNodeViewModel>().FirstOrDefault();
                    if (producerNode != null)
                    {
                        activeResourceBridges.Add(comp.Resource.Uid);
                        if (comp.CurrentRecipe.OutputResourceQuantities.TryGetValue(comp.Resource.Uid, out double totalOut) && totalOut != 0)
                        {
                            double nextScale = childNode.AbsCost / totalOut;
                            ProcessRecipeNode(producerNode, nextScale, activeResourceBridges);
                        }
                        activeResourceBridges.Remove(comp.Resource.Uid);
                    }
                }
            }
        }
    }
}