using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

public interface IResourceSearchService
{
    ObservableCollection<ResourceItemViewModel> Resources { get; }
    string SearchText { get; set; }
    Predicate<object> SearchByName { get; }
}
