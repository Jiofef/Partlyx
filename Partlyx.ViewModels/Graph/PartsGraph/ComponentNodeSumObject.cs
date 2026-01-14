using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentNodeSumObject : SumHierarchyObject, IDisposable
    {
        private readonly IDisposable _logicSubscription;
        private readonly IDisposable _componentChangedSubscription;
        private readonly IDisposable _componentResourceChangedSubscription;

        public ComponentGraphNodeViewModel ComponentNode { get; }

        private RecipeComponentViewModel? _component;
        public RecipeComponentViewModel? Component { get => _component; private set => SetProperty(ref _component, value); }

        private ResourceViewModel? _componentResource;
        public ResourceViewModel? ComponentResource { get => _componentResource; private set => SetProperty(ref _componentResource, value); }

        public ComponentNodeSumObject(ComponentGraphNodeViewModel componentNode)
        {
            ComponentNode = componentNode;

            // Subscribe to Value to update Component reference
            _componentChangedSubscription = ComponentNode
                .WhenAnyValue(n => n.Value)
                .Subscribe(_ => Component = ComponentNode.Value as RecipeComponentViewModel);

            // Subscribe to LinkedResource change
            _componentResourceChangedSubscription = this
                .WhenAnyValue(c => c.Component!.LinkedResource!.Value)
                .Subscribe(_ => ComponentResource = Component?.LinkedResource?.Value);

            // Combined subscription for Cost and IsOutput to update BaseValue
            _logicSubscription = this
                .WhenAnyValue(x => x.ComponentNode.Cost)
                .Subscribe(cost => BaseValue = cost);
        }

        new public void Dispose()
        {
            base.Dispose();
            _logicSubscription.Dispose();
            _componentChangedSubscription.Dispose();
            _componentResourceChangedSubscription.Dispose();
        }
    }
}