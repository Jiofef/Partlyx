using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class RecipesVMContainer : IRecipesVMContainer, IIsolatedRecipesVMContainer, IGlobalRecipesVMContainer
    {
        public RecipesVMContainer() { }

        public ObservableCollection<RecipeViewModel> Recipes { get; } = new();
        public SourceList<RecipeViewModel> RecipesSourceList { get; } = new();

        public void ClearRecipes()
        {
            RecipesSourceList.Clear();
            Recipes.Clear();
        }
        public void AddRecipe(RecipeViewModel recipe)
        {
            RecipesSourceList.Add(recipe);
            Recipes.Add(recipe);
        }
        public void RemoveRecipe(RecipeViewModel recipe)
        {
            RecipesSourceList.Remove(recipe);
            Recipes.Remove(recipe);
        }
    }
}
