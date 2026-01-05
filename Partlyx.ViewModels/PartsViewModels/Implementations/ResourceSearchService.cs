using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Partlyx.Infrastructure.Data.Implementations;
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

        // DynamicData container for resources
        private readonly SourceCache<ResourceViewModel, Guid> _resourcesCache = new(r => r.Uid);

        private readonly ReadOnlyObservableCollection<ResourceViewModel> _filteredResources;
        public ReadOnlyObservableCollection<ResourceViewModel> FilteredResources => _filteredResources;
        public ReadOnlyObservableCollection<ResourceViewModel> UnfilteredResources { get; }

        [ObservableProperty]
        private string _searchText = "";

        public ResourceSearchService(IGlobalResourcesVMContainer grvmc)
        {
            _resourcesContainer = grvmc;

            UnfilteredResources = new(_resourcesContainer.Resources);

            _resourcesContainer.Resources
                .ToObservableChangeSet(r => r.Uid)
                .PopulateInto(_resourcesCache);

            var filterPredicate = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(150))
                .Select(BuildFilter);

            _resourcesCache.Connect()
                .Filter(filterPredicate)
                .SortAndBind(out _filteredResources, SortExpressionComparer<ResourceViewModel>.Ascending(r => r.Name))
                .DisposeMany()
                .Subscribe();
        }

        private Func<ResourceViewModel, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return _ => true;

            return rItem =>
                rItem.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || (rItem.LinkedDefaultRecipe?.Value is RecipeViewModel rc && rc.Inputs.Any(c =>
                        c.LinkedResource?.Value?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
        }
    }
}