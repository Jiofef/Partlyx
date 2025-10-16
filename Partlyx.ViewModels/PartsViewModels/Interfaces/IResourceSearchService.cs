using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

public interface IResourceSearchService
{
    ObservableCollection<ResourceViewModel> Resources { get; }
    string SearchText { get; set; }
    Predicate<object> SearchByName { get; }
}
