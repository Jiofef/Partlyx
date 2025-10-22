using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.Graph
{
    public class ComponentNodeSumObject : SumHierarchyObject, IDisposable
    {
        private readonly IDisposable _costChangedSubscription;
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

            _costChangedSubscription = ComponentNode
                .WhenAnyValue(x => x.Cost)
                .Subscribe(_ => BaseValue = ComponentNode.Cost);

            _componentChangedSubscription = ComponentNode
                .WhenAnyValue(n => n.Value)
                .Subscribe(_ => Component = ComponentNode.Value as RecipeComponentViewModel);

            _componentResourceChangedSubscription = this
                .WhenAnyValue(c => c.Component.LinkedResource.Value)
                .Subscribe(_ => ComponentResource = Component?.LinkedResource?.Value);
        }

        new public void Dispose()
        {
            base.Dispose();

            _costChangedSubscription.Dispose();
            _componentChangedSubscription.Dispose();
            _componentResourceChangedSubscription.Dispose();
        }
    }
}