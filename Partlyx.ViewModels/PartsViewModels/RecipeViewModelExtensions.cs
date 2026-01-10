using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.PartsViewModels
{
    public static class RecipeViewModelExtensions
    {
        #region Conversions

        /// <summary>
        /// Calculates how many times the recipe must be executed (the "scale") to process a specific amount of a component.
        /// </summary>
        /// <param name="isOutput">True if the component is an Output, False if it is an Input.</param>
        public static double GetCraftsCount(this RecipeViewModel recipe, Guid componentUid, double amount, bool isOutput)
        {
            var dictionary = isOutput ? recipe.OutputsDic : recipe.InputsDic;

            if (dictionary.TryGetValue(componentUid, out var component) && component.Quantity != 0)
            {
                // CraftsCount = Have / Recipe Requirement
                return amount / component.Quantity;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the amount of a specific component based on the number of recipe executions (crafts).
        /// </summary>
        /// <param name="isOutput">True to look for the component in Outputs, False for Inputs.</param>
        public static double GetComponentAmountFromCrafts(this RecipeViewModel recipe, Guid componentUid, double craftsCount, bool isOutput)
        {
            var dictionary = isOutput ? recipe.OutputsDic : recipe.InputsDic;

            if (dictionary.TryGetValue(componentUid, out var component))
            {
                // Total Amount = Crafts * Recipe Base Quantity
                return craftsCount * component.Quantity;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the quantity of a target component based on the quantity of a source component within the same recipe.
        /// </summary>
        /// <param name="isForward">True if Source is Input and Target is Output. False if vice versa.</param>
        public static double GetComponentAmountFromOtherComponent(this RecipeViewModel recipe, Guid sourceComponentUid, Guid targetComponentUid, double sourceAmount, bool isForward = true)
        {
            // First, find the "scale" of the recipe based on the source
            double crafts = recipe.GetCraftsCount(sourceComponentUid, sourceAmount, !isForward);

            if (crafts == 0) return 0;

            // Then, get the target amount using that scale
            return recipe.GetComponentAmountFromCrafts(targetComponentUid, crafts, isForward);
        }

        #endregion

        #region Analysis Helpers

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

        public static List<ResourceAmountPairViewModel> GetMerged(this ICollection<RecipeComponentViewModel> list)
        {
            return list
                .GroupBy(c => c.Resource)
                .Where(g => g.Key != null)
                .Select(g => new ResourceAmountPairViewModel(g.Key!, g.Sum(c => c.Quantity)))
                .ToList();
        }

        /// <summary>
        /// Returns only those components whose resources appear more than once in the collection.
        /// Useful for identifying shared resources or potential loops.
        /// </summary>
        public static List<RecipeComponentViewModel> GetWithoutUniqueComponents(this IEnumerable<RecipeComponentViewModel> list)
        {
            return list
                .GroupBy(c => c.Resource)
                .Where(g => g.Key != null && g.Count() > 1)
                .SelectMany(g => g)
                .ToList();
        }

        /// <summary>
        /// Traverses the entire recipe tree, preventing eternal recursions. The bool value indicates whether the current component causes eternal recursion.
        /// </summary>
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

        #endregion
    }
}