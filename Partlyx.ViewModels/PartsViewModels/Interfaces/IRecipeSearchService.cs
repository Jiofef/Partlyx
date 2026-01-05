using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

public interface IRecipeSearchService
{
    string SearchText { get; set; }
    ReadOnlyObservableCollection<RecipeViewModel> FilteredRecipes { get; }
    ReadOnlyObservableCollection<RecipeViewModel> UnfilteredRecipes { get; }
}
