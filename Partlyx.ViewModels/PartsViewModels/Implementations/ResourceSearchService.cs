using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceSearchService : ObservableObject, IResourceSearchService
    {
        private readonly IGlobalResourcesVMContainer _resourcesContainer;

        private readonly ReadOnlyObservableCollection<ResourceViewModel> _filteredResources;

        public ReadOnlyObservableCollection<ResourceViewModel> UnfilteredResources { get; }
        public ReadOnlyObservableCollection<ResourceViewModel> FilteredResources => _filteredResources;

        public ResourceSearchService(IGlobalResourcesVMContainer grvmc)
        {
            _resourcesContainer = grvmc;

            UnfilteredResources = new(_resourcesContainer.Resources);

            var filter = this.WhenAnyValue(x => x.SearchText)
                .Select(search => new Func<ResourceViewModel, bool>(r => SearchByName(r)));

            _resourcesContainer.Resources
                .ToObservableChangeSet()
                .Filter(filter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filteredResources)
                .DisposeMany()
                .Subscribe();
        }

        public Predicate<ResourceViewModel> SearchByName => rItem =>
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return rItem.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || rItem.Recipes.Any(rc => rc.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                || rItem.Recipes.Any(rc => rc.Components.Any(c => c.LinkedResource?.Value != null && c.LinkedResource.Value.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        };

        [ObservableProperty]
        private string _searchText = "";
    }

}