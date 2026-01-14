using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class RecipeComponentPath : Path<RecipeComponentViewModel>
    {
        public RecipeComponentPath(LinkedList<RecipeComponentViewModel> nodes) : base(nodes)
        {
        }

        public int GetComponentsAmount() => Steps.Count;
        public int GetRecipesAmount() => Steps.Count / 2;

        public static RecipeComponentPath FromList(List<RecipeComponentViewModel> path)
        {
            var linkedList = new LinkedList<RecipeComponentViewModel>(path);
            return new RecipeComponentPath(linkedList);
        }

        public Dictionary<RecipeComponentViewModel, double> CalculateMultipliers()
        {
            var multipliers = new Dictionary<RecipeComponentViewModel, double>();
            double currentMultiplier = 1.0;

            var current = GetFirst();
            while (current != null)
            {
                multipliers[current.Value] = currentMultiplier;
                currentMultiplier *= current.Value.Quantity;
                current = GetNext(current);
            }

            return multipliers;
        }

        public Dictionary<ResourceViewModel, double> Quantify(double inputAmount)
        {
            var totals = new Dictionary<ResourceViewModel, double>();
            double currentFlow = inputAmount;
            var currentNode = Steps.First;

            while (currentNode != null && currentNode.Next != null)
            {
                var pathInputComp = currentNode.Value;
                var pathOutputComp = currentNode.Next.Value;
                var recipe = pathInputComp.ParentRecipe;

                if (recipe == null) break;

                // FIX: Detect if we are moving backwards through the recipe
                // A step is "reverse" if the first component of the pair is actually an Output in the recipe definition
                bool isReverseStep = recipe.Outputs.Any(o => o.Uid == pathInputComp.Uid);

                // 1. Calculate crafts count based on the direction
                // If forward: calculate from input amount (false). If reverse: calculate from output amount (true).
                double crafts = recipe.GetCraftsCount(pathInputComp.Uid, currentFlow, isReverseStep);

                // 2. Add all components to summary with direction-aware signs
                // In forward: consume Inputs (-), produce Outputs (+)
                // In reverse: produce Inputs (+), consume Outputs (-)
                double directionMultiplier = isReverseStep ? -1.0 : 1.0;

                foreach (var inComp in recipe.Inputs)
                {
                    if (inComp.Resource != null)
                        AddDelta(totals, inComp.Resource, -inComp.Quantity * crafts * directionMultiplier);
                }
                foreach (var outComp in recipe.Outputs)
                {
                    if (outComp.Resource != null)
                        AddDelta(totals, outComp.Resource, outComp.Quantity * crafts * directionMultiplier);
                }

                // 3. Update flow for the next recipe in chain
                currentFlow = pathOutputComp.Quantity * crafts;

                currentNode = currentNode.Next.Next;
            }

            // Cleanup small values
            const double epsilon = 1e-10;
            foreach (var key in totals.Keys.ToList())
                if (Math.Abs(totals[key]) < epsilon) totals.Remove(key);

            return totals;
        }

        public Dictionary<ResourceViewModel, double> QuantifyFromOutputAmount(double targetOutputAmount)
        {
            double neededFlow = CalculateRequiredInput(targetOutputAmount);

            return Quantify(neededFlow);
        }

        public double CalculateRequiredInput(double targetOutputAmount)
        {
            double neededFlow = targetOutputAmount;
            var currentNode = Steps.Last;

            while (currentNode != null && currentNode.Previous != null)
            {
                var pathOutputComp = currentNode.Value;
                var pathInputComp = currentNode.Previous.Value;
                var recipe = pathOutputComp.ParentRecipe;

                if (recipe != null)
                {
                    // Detect direction for the backward pass
                    bool isReverseStep = recipe.Outputs.Any(o => o.Uid == pathInputComp.Uid);

                    // Logic for needed flow also needs to be direction-aware
                    // If step is reverse (Out -> In), then pathOutputComp is an Input. 
                    // So we calculate crafts needed to get that Input amount.
                    double crafts = recipe.GetCraftsCount(pathOutputComp.Uid, neededFlow, !isReverseStep);
                    neededFlow = pathInputComp.Quantity * crafts;
                }

                currentNode = currentNode.Previous.Previous;
            }

            return neededFlow;
        }

        private void AddDelta(Dictionary<ResourceViewModel, double> dict, ResourceViewModel res, double delta)
        {
            if (dict.ContainsKey(res)) dict[res] += delta;
            else dict[res] = delta;
        }

        public IEnumerable<ResourceAmountPairViewModel> QuantifyToValuePairs(double inputAmount)
        {
            var quantified = Quantify(inputAmount);
            return quantified.Select(kvp => new ResourceAmountPairViewModel(kvp.Key, kvp.Value));
        }

        public IEnumerable<ResourceAmountPairViewModel> QuantifyFromOutputAmountToValuePairs(double outputAmount)
        {
            var quantified = QuantifyFromOutputAmount(outputAmount);
            return quantified.Select(kvp => new ResourceAmountPairViewModel(kvp.Key, kvp.Value));
        }
    }
}