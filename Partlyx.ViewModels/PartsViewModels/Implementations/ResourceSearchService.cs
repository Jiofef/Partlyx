using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceSearchService : ObservableObject, IResourceSearchService
    {
        private readonly IGlobalResourcesVMContainer _resourcesContainer;
        public ObservableCollection<ResourceViewModel> Resources => _resourcesContainer.Resources;

        public ResourceSearchService(IGlobalResourcesVMContainer grvmc)
        {
            _resourcesContainer = grvmc;
        }

        [ObservableProperty]
        private string _searchText = "";

        public Predicate<object> SearchByName => o =>
        {
            if (o is not ResourceViewModel rItem) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return rItem.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true;
        };

        partial void OnSearchTextChanged(string? oldValue, string newValue)
        {
            OnPropertyChanged(nameof(SearchByName));
        }
    }
}
