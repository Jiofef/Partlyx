using Partlyx.Core.Partlyx;
using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels
{
    public static class RecipeViewModelExtensions
    {
        public static List<ResourceAmountPairViewModel> GetQuantifiedList(this RecipeViewModel recipe)
        {
            var fundamentComponents = new List<RecipeComponentViewModel>();

            recipe.TraverseSafe((component, isCycling) =>
            {
                if (isCycling || (component.CurrentRecipe?.Inputs).IsNullOrEmpty())
                    fundamentComponents.Add(component);
            });

            return fundamentComponents.GetMerged();
        }
        public static List<RecipeComponentViewModel> GetWithoutUniqueComponents(this ICollection<RecipeComponentViewModel> list)
        {
            var groupedNotUniqueComponents = list
                .GroupBy(c => c.Resource)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .ToList();

            return groupedNotUniqueComponents;
        }
        public static List<ResourceAmountPairViewModel> GetMerged(this ICollection<RecipeComponentViewModel> list)
        {
            List<ResourceViewModel> uniqueResourcesList = new();
            Dictionary<ResourceViewModel, double> resourceAmounts = new();

            foreach (var component in list)
            {
                var resource = component.Resource;

                if (resource == null) continue;

                if (resourceAmounts.ContainsKey(resource))
                    resourceAmounts[resource] += component.Quantity;
                else
                {
                    uniqueResourcesList.Add(resource);
                    resourceAmounts.Add(resource, component.Quantity);
                }
            }

            return uniqueResourcesList.Select(r => new ResourceAmountPairViewModel(r, resourceAmounts[r])).ToList();
        }

        public static List<ResourceAmountPairViewModel> GetMergedComponentsList(this RecipeViewModel recipe)
            => GetMerged(recipe.Inputs);


        // Traverses the entire recipe tree, preventing eternal recursions. The bool value indicates whether the current component causes eternal recursion.
        public static void TraverseSafe(this RecipeViewModel recipe, Action<RecipeComponentViewModel, bool> action)
        {
            HashSet<ResourceViewModel> parentResources = new();

            parentResources.TryAddIfNotNull(recipe.LinkedParentResource?.Value);
            Iterate(recipe.Inputs);

            void Iterate(IList<RecipeComponentViewModel> components)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    var componentResource = component.Resource;

                    bool isComponentRecursive = parentResources.Contains(componentResource);

                    action(component, isComponentRecursive);

                    if (component.CurrentRecipe != null && component.CurrentRecipe.Inputs.Count > 0 && !isComponentRecursive)
                    {
                        parentResources.Add(componentResource);
                        Iterate(component.CurrentRecipe.Inputs);
                    }

                    parentResources.Remove(componentResource);
                }
            }
        }
    }
}
