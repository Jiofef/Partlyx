using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeSearchService : ObservableObject, IRecipeSearchService
    {
        private readonly IGlobalRecipesVMContainer _recipesContainer;

        // DynamicData container for recipes
        private readonly SourceCache<RecipeViewModel, Guid> _recipesCache = new(r => r.Uid);

        private readonly ReadOnlyObservableCollection<RecipeViewModel> _filteredRecipes;
        public ReadOnlyObservableCollection<RecipeViewModel> FilteredRecipes => _filteredRecipes;
        public ReadOnlyObservableCollection<RecipeViewModel> UnfilteredRecipes { get; }

        [ObservableProperty]
        private string _searchText = "";

        public RecipeSearchService(IGlobalRecipesVMContainer grvmc)
        {
            _recipesContainer = grvmc;

            UnfilteredRecipes = new(_recipesContainer.Recipes);

            _recipesContainer.Recipes
                .ToObservableChangeSet(r => r.Uid)
                .PopulateInto(_recipesCache);

            var filterPredicate = this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(150))
                .Select(BuildFilter);

            _recipesCache.Connect()
                .Filter(filterPredicate)
                .SortAndBind(out _filteredRecipes, SortExpressionComparer<RecipeViewModel>.Ascending(r => r.Name))
                .DisposeMany()
                .Subscribe();
        }

        private Func<RecipeViewModel, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return _ => true;

            return rItem =>
                rItem.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || rItem.Inputs.Any(c => c.LinkedResource?.Value?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                || rItem.Outputs.Any(c => c.LinkedResource?.Value?.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}