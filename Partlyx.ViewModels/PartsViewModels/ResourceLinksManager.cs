using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels
{
    /// <summary>
    /// Manages resource link tracking for a recipe.
    /// Tracks which resources are present in inputs/outputs and their link types.
    /// </summary>
    public class ResourceLinksManager
    {
        private readonly Dictionary<Guid, int> _inputCounts = new();
        private readonly Dictionary<Guid, int> _outputCounts = new();
        private readonly HashSet<Guid> _inputResources = new();
        private readonly HashSet<Guid> _outputResources = new();
        private readonly IEventBus _bus;
        private readonly RecipeViewModel _recipe;

        public ResourceLinksManager(RecipeViewModel recipe, IEventBus bus)
        {
            _recipe = recipe;
            _bus = bus;
        }

        /// <summary>
        /// Gets the set of resource Uids present in inputs
        /// </summary>
        public HashSet<Guid> InputResources => _inputResources;

        /// <summary>
        /// Gets the set of resource Uids present in outputs
        /// </summary>
        public HashSet<Guid> OutputResources => _outputResources;

        /// <summary>
        /// Checks if a resource is present in inputs
        /// </summary>
        public bool HasResourceInInputs(Guid resourceUid) => _inputResources.Contains(resourceUid);

        /// <summary>
        /// Checks if a resource is present in outputs
        /// </summary>
        public bool HasResourceInOutputs(Guid resourceUid) => _outputResources.Contains(resourceUid);

        /// <summary>
        /// Adds a component's resource to the appropriate collection
        /// </summary>
        public void AddComponent(RecipeComponentType type, Guid resourceUid)
        {
            if (resourceUid == Guid.Empty) return;

            bool wasPresent;
            if (type == RecipeComponentType.Input)
            {
                wasPresent = _inputResources.Contains(resourceUid);
                _inputCounts[resourceUid] = _inputCounts.GetValueOrDefault(resourceUid) + 1;
                _inputResources.Add(resourceUid);
            }
            else
            {
                wasPresent = _outputResources.Contains(resourceUid);
                _outputCounts[resourceUid] = _outputCounts.GetValueOrDefault(resourceUid) + 1;
                _outputResources.Add(resourceUid);
            }

            // Publish event if presence changed
            if (!wasPresent)
            {
                PublishResourceLinkChanged(resourceUid);
            }
        }

        /// <summary>
        /// Removes a component's resource from the appropriate collection
        /// </summary>
        public void RemoveComponent(RecipeComponentType type, Guid resourceUid)
        {
            if (resourceUid == Guid.Empty) return;

            if (type == RecipeComponentType.Input)
            {
                if (_inputCounts.TryGetValue(resourceUid, out int count))
                {
                    if (count > 1)
                    {
                        _inputCounts[resourceUid] = count - 1;
                    }
                    else
                    {
                        _inputCounts.Remove(resourceUid);
                        _inputResources.Remove(resourceUid);
                        PublishResourceLinkChanged(resourceUid);
                    }
                }
            }
            else
            {
                if (_outputCounts.TryGetValue(resourceUid, out int count))
                {
                    if (count > 1)
                    {
                        _outputCounts[resourceUid] = count - 1;
                    }
                    else
                    {
                        _outputCounts.Remove(resourceUid);
                        _outputResources.Remove(resourceUid);
                        PublishResourceLinkChanged(resourceUid);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the link type for a resource based on its presence in inputs/outputs
        /// </summary>
        public RecipeResourceLinkTypeEnum GetLinkTypeForResource(Guid resourceUid)
        {
            bool inInputs = _inputResources.Contains(resourceUid);
            bool inOutputs = _outputResources.Contains(resourceUid);

            if (_recipe.IsReversible)
            {
                if (inInputs || inOutputs) return RecipeResourceLinkTypeEnum.Both;
                return RecipeResourceLinkTypeEnum.None;
            }
            else
            {
                if (inInputs && inOutputs) return RecipeResourceLinkTypeEnum.Both;
                if (inInputs) return RecipeResourceLinkTypeEnum.Receiving;
                if (inOutputs) return RecipeResourceLinkTypeEnum.Producing;
                return RecipeResourceLinkTypeEnum.None;
            }
        }

        /// <summary>
        /// Publishes a resource link changed event
        /// </summary>
        public void PublishResourceLinkChanged(Guid resourceUid)
        {
            var linkType = GetLinkTypeForResource(resourceUid);
            var ev = new RecipeResourceLinkChangedEvent(_recipe, resourceUid, linkType);
            _bus.Publish(ev);
        }

        /// <summary>
        /// Publishes resource link changed events for all tracked resources
        /// </summary>
        public void PublishAllResourceLinkChanges()
        {
            var allResources = new HashSet<Guid>(_inputResources);
            allResources.UnionWith(_outputResources);

            foreach (var resourceUid in allResources)
            {
                PublishResourceLinkChanged(resourceUid);
            }
        }

        /// <summary>
        /// Initializes from existing components
        /// </summary>
        public void InitializeFromComponents(IEnumerable<RecipeComponentViewModel> inputs, IEnumerable<RecipeComponentViewModel> outputs)
        {
            foreach (var component in inputs)
            {
                var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
                if (resourceUid != Guid.Empty)
                {
                    _inputCounts[resourceUid] = _inputCounts.GetValueOrDefault(resourceUid) + 1;
                    _inputResources.Add(resourceUid);
                }
            }

            foreach (var component in outputs)
            {
                var resourceUid = component.LinkedResource?.Uid ?? Guid.Empty;
                if (resourceUid != Guid.Empty)
                {
                    _outputCounts[resourceUid] = _outputCounts.GetValueOrDefault(resourceUid) + 1;
                    _outputResources.Add(resourceUid);
                }
            }
        }
    }
}
