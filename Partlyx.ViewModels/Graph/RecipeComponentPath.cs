using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph
{
    public class RecipeComponentPath : Path<RecipeComponentViewModel>
    {
        public RecipeComponentPath(LinkedList<RecipeComponentViewModel> nodes) : base(nodes)
        {
        }

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
            var multipliers = CalculateMultipliers();
            var resourceAmounts = new Dictionary<ResourceViewModel, double>();

            double scaleFactor = inputAmount / (GetFirst()?.Value.Quantity ?? 1.0);

            foreach (var kvp in multipliers)
            {
                var component = kvp.Key;
                double multiplier = kvp.Value * scaleFactor;

                double amount = multiplier * component.Quantity;

                // For inputs, it's cost (negative), for outputs - production (positive)
                if (!component.IsOutput)
                {
                    amount = -amount;
                }

                if (resourceAmounts.ContainsKey(component.Resource))
                {
                    resourceAmounts[component.Resource] += amount;
                }
                else
                {
                    resourceAmounts[component.Resource] = amount;
                }
            }

            return resourceAmounts;
        }
    }
}
