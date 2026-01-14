using DynamicData.Binding;
using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentNodeSumCompilation : SumHierarchyObject
    {
        private readonly List<IDisposable> _subscriptions = new();

        private ResourceViewModel? _componentsResource;
        public ResourceViewModel? ComponentsResource { get => _componentsResource; set => SetProperty(ref _componentsResource, value); }

        public ComponentNodeSumCompilation(ResourceViewModel componentResource)
        {
            _componentsResource = componentResource;

            var componentResourceNameUpdateSubscription = this
                .WhenAnyValue(c => c.ComponentsResource.Name)
                .Subscribe(n => UpdateColumnText());
            _subscriptions.Add(componentResourceNameUpdateSubscription);

            var componentsSumChangedSubscription = this.
                WhenAnyValue(c => c.Sum)
                .Subscribe(n => UpdateBottomColumnText());
            _subscriptions.Add(componentsSumChangedSubscription);

            UpdateColumnText();
            UpdateBottomColumnText();
        }

        private string _columnText = "";
        public string ColumnText { get => _columnText; set => SetProperty(ref _columnText, value); }

        private void UpdateColumnText()
        {
            string newText = _componentsResource?.Name ?? "???";
            ColumnText = newText;
        }

        private string _bottomColumnText = "";
        public string BottomColumnText { get => _bottomColumnText; set => SetProperty(ref _bottomColumnText, value); }

        private void UpdateBottomColumnText()
        {
            string newText = $"X{Math.Abs(Sum)}";
            BottomColumnText = newText;
        }

        new public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}