using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph
{
    public class RecipeComponentPath : Path<RecipeComponentViewModel>
    {
        public RecipeComponentPath(LinkedList<RecipeComponentViewModel> nodes) : base(nodes)
        {
        }

        public int GetComponentsAmount() => Nodes.Count;
        public int GetRecipesAmount() => Nodes.Count / 2;

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

            // This tracks the amount of the current resource moving through the path
            double currentFlow = inputAmount;

            // We iterate through the path nodes in pairs (Recipe Input -> Recipe Output)
            var currentNode = Nodes.First;

            while (currentNode != null && currentNode.Next != null)
            {
                var pathInputComp = currentNode.Value;      // e.g., 1 Oak
                var pathOutputComp = currentNode.Next.Value; // e.g., 4 Planks
                var recipe = pathInputComp.ParentRecipe;

                if (recipe == null) break;

                // 1. Determine how many times this specific recipe is executed
                // based on the amount of resource arriving at this step.
                double crafts = recipe.GetCraftsCount(pathInputComp.Uid, currentFlow, false);

                // 2. Add ALL inputs/outputs of this recipe to the global summary
                // This captures side-products and auxiliary requirements
                foreach (var inComp in recipe.Inputs)
                {
                    if (inComp.Resource != null)
                        AddDelta(totals, inComp.Resource, -inComp.Quantity * crafts);
                }
                foreach (var outComp in recipe.Outputs)
                {
                    if (outComp.Resource != null)
                        AddDelta(totals, outComp.Resource, outComp.Quantity * crafts);
                }

                // 3. Update currentFlow for the NEXT step in the path.
                // The flow for the next recipe is the scaled amount of our current output.
                // e.g., if we produced 4 planks, currentFlow becomes 4 for the next step.
                currentFlow = pathOutputComp.Quantity * crafts;

                // Move to the next "link" (the next pair in the chain)
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
            // To find out how much input is needed, we traverse the path backwards
            double neededFlow = targetOutputAmount;
            var currentNode = Nodes.Last;

            while (currentNode != null && currentNode.Previous != null)
            {
                var pathOutputComp = currentNode.Value;
                var pathInputComp = currentNode.Previous.Value;
                var recipe = pathOutputComp.ParentRecipe;

                if (recipe != null)
                {
                    // Backwards: How many crafts to get 'neededFlow' of this output?
                    double crafts = recipe.GetCraftsCount(pathOutputComp.Uid, neededFlow, true);
                    // How much input did that require?
                    neededFlow = pathInputComp.Quantity * crafts;
                }

                currentNode = currentNode.Previous.Previous;
            }

            // Now that we know the initial input required, run the forward calculation
            return Quantify(neededFlow);
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
    public enum ComponentPathAmountMode { Input, Output}
}