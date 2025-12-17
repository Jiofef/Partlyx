using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

public interface IResourceSearchService
{
    string SearchText { get; set; }
    ReadOnlyObservableCollection<ResourceViewModel> FilteredResources { get; }
    ReadOnlyObservableCollection<ResourceViewModel> UnfilteredResources { get; }
}
