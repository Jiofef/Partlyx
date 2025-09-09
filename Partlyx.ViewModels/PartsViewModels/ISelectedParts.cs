using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels
{
    public interface ISelectedParts
    {
        ObservableCollection<ResourceItemViewModel> Resources { get; }
        ObservableCollection<RecipeItemViewModel> Recipes { get; }
        ObservableCollection<RecipeComponentItemViewModel> Components { get; }

        void AddComponentToSelected(RecipeComponentItemViewModel component);
        void AddRecipeToSelected(RecipeItemViewModel recipe);
        void AddResourceToSelected(ResourceItemViewModel resource);
        void ClearSelectedComponents();
        void ClearSelectedRecipes();
        void ClearSelectedResources();
        RecipeComponentItemViewModel? GetSingleComponentOrNull();
        RecipeItemViewModel? GetSingleRecipeOrNull();
        ResourceItemViewModel? GetSingleResourceOrNull();
        void SelectSingleComponent(RecipeComponentItemViewModel component);
        void SelectSingleRecipe(RecipeItemViewModel recipe);
        void SelectSingleResource(ResourceItemViewModel resource);
    }
}