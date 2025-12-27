using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IRecipesVMContainer
    {
        ObservableCollection<RecipeViewModel> Recipes { get; }
        SourceList<RecipeViewModel> RecipesSourceList { get; }

        void AddRecipe(RecipeViewModel recipe);
        void ClearRecipes();
        void RemoveRecipe(RecipeViewModel recipe);
    }

    public interface IIsolatedRecipesVMContainer : IRecipesVMContainer { }

    public interface IGlobalRecipesVMContainer : IRecipesVMContainer { }
}
