using DynamicData.Binding;
using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIServices;
using ReactiveUI;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentNodeSumCompilation : SumHierarchyObject
    {
        private readonly List<IDisposable> _subscriptions = new();
        private readonly ApplicationSettingsProviderViewModel _settingsProvider;

        private ResourceViewModel? _componentsResource;
        public ResourceViewModel? ComponentsResource { get => _componentsResource; set => SetProperty(ref _componentsResource, value); }

        public ComponentNodeSumCompilation(ResourceViewModel componentResource, ApplicationSettingsProviderViewModel settingsProvider)
        {
            _componentsResource = componentResource;
            _settingsProvider = settingsProvider;

            var componentResourceNameUpdateSubscription = this
                .WhenAnyValue(c => c.ComponentsResource.Name)
                .Subscribe(_ => UpdateColumnText());
            _subscriptions.Add(componentResourceNameUpdateSubscription);

            var componentsSumChangedSubscription = this
                .WhenAnyValue(c => c.Sum)
                .Subscribe(_ => UpdateBottomColumnText());
            _subscriptions.Add(componentsSumChangedSubscription);

            var decimalPlacesChangedSubscription = _settingsProvider
                .WhenAnyValue(p => p.DecimalPlacesInGraphSums)
                .Subscribe(_ => UpdateBottomColumnText());

            UpdateColumnText();
            UpdateBottomColumnText();
        }

        public ComponentNodeSumCompilation(ResourceViewModel componentResource) : this(componentResource, componentResource.GlobalInfo.ApplicationSettings) { }

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
            const int MAX_DECIMAL_PLACES = 16;
            int decimalPlaces = Math.Min(_settingsProvider.DecimalPlacesInGraphSums, MAX_DECIMAL_PLACES);
            var costStringMap = InfoVisualisationHelper.GetMappingForFloatsString(decimalPlaces);
            string newText = $"X{Math.Abs(Sum).ToString(costStringMap)}";
            BottomColumnText = newText;
        }

        new public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}