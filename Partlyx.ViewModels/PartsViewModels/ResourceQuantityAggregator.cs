using Partlyx.Core.Partlyx;
using System.Collections.ObjectModel;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels
{
    /// <summary>
    /// Aggregates and tracks resource quantities for a recipe.
    /// Handles sum calculations for components with the same resource.
    /// </summary>
    public class ResourceQuantityAggregator
    {
        private readonly RecipeViewModel _recipe;

        // Input resource quantity sums
        private readonly Dictionary<Guid, double> _inputQuantities = new();
        // Output resource quantity sums
        private readonly Dictionary<Guid, double> _outputQuantities = new();

        public ResourceQuantityAggregator(RecipeViewModel recipe)
        {
            _recipe = recipe;
        }

        /// <summary>
        /// Gets the readonly dictionary of input resource quantities
        /// </summary>
        public ReadOnlyDictionary<Guid, double> InputQuantities => new(_inputQuantities);

        /// <summary>
        /// Gets the readonly dictionary of output resource quantities
        /// </summary>
        public ReadOnlyDictionary<Guid, double> OutputQuantities => new(_outputQuantities);

        /// <summary>
        /// Gets the total quantity for a resource in inputs
        /// </summary>
        public double GetInputQuantity(Guid resourceUid) 
            => _inputQuantities.TryGetValue(resourceUid, out var value) ? value : 0;

        /// <summary>
        /// Gets the total quantity for a resource in outputs
        /// </summary>
        public double GetOutputQuantity(Guid resourceUid) 
            => _outputQuantities.TryGetValue(resourceUid, out var value) ? value : 0;

        /// <summary>
        /// Adds a component's quantity to the appropriate sum
        /// </summary>
        public void AddComponent(RecipeComponentViewModel component)
        {
            var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
            if (resourceUid == Guid.Empty) return;

            var dict = component.IsOutput ? _outputQuantities : _inputQuantities;
            if (dict.TryGetValue(resourceUid, out double currentSum))
            {
                dict[resourceUid] = currentSum + component.Quantity;
            }
            else
            {
                dict[resourceUid] = component.Quantity;
            }
        }

        /// <summary>
        /// Removes a component's quantity from the appropriate sum
        /// </summary>
        public void RemoveComponent(RecipeComponentViewModel component)
        {
            var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
            if (resourceUid == Guid.Empty) return;

            var dict = component.IsOutput ? _outputQuantities : _inputQuantities;
            UpdateQuantityDelta(dict, resourceUid, -component.Quantity);
        }

        /// <summary>
        /// Updates quantity when a component's quantity changes
        /// </summary>
        public void UpdateComponentQuantity(RecipeComponentType componentType, Guid resourceUid, double oldQuantity, double newQuantity)
        {
            if (resourceUid == Guid.Empty) return;

            var dict = componentType == RecipeComponentType.Output ? _outputQuantities : _inputQuantities;
            UpdateQuantityDelta(dict, resourceUid, newQuantity - oldQuantity);
        }

        /// <summary>
        /// Handles component moving from one recipe to another
        /// </summary>
        public void HandleComponentMoved(RecipeComponentViewModel component, bool wasOutput, bool isNowOutput)
        {
            var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
            if (resourceUid == Guid.Empty) return;

            // Remove from old type
            var oldDict = wasOutput ? _outputQuantities : _inputQuantities;
            UpdateQuantityDelta(oldDict, resourceUid, -component.Quantity);

            // Add to new type
            var newDict = isNowOutput ? _outputQuantities : _inputQuantities;
            if (newDict.TryGetValue(resourceUid, out double currentSum))
            {
                newDict[resourceUid] = currentSum + component.Quantity;
            }
            else
            {
                newDict[resourceUid] = component.Quantity;
            }
        }

        /// <summary>
        /// Initializes quantities from existing components
        /// </summary>
        public void InitializeFromComponents(IEnumerable<RecipeComponentViewModel> inputs, IEnumerable<RecipeComponentViewModel> outputs)
        {
            foreach (var component in inputs)
            {
                var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
                if (resourceUid == Guid.Empty) continue;

                if (_inputQuantities.TryGetValue(resourceUid, out double currentSum))
                {
                    _inputQuantities[resourceUid] = currentSum + component.Quantity;
                }
                else
                {
                    _inputQuantities[resourceUid] = component.Quantity;
                }
            }

            foreach (var component in outputs)
            {
                var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
                if (resourceUid == Guid.Empty) continue;

                if (_outputQuantities.TryGetValue(resourceUid, out double currentSum))
                {
                    _outputQuantities[resourceUid] = currentSum + component.Quantity;
                }
                else
                {
                    _outputQuantities[resourceUid] = component.Quantity;
                }
            }
        }

        private static void UpdateQuantityDelta(Dictionary<Guid, double> dict, Guid resourceUid, double delta)
        {
            const double epsilon = 1e-9;

            if (dict.TryGetValue(resourceUid, out double currentSum))
            {
                double newSum = currentSum + delta;
                if (newSum > epsilon)
                {
                    dict[resourceUid] = newSum;
                }
                else
                {
                    dict.Remove(resourceUid);
                }
            }
            else if (delta > 0)
            {
                dict[resourceUid] = delta;
            }
        }
    }
}
