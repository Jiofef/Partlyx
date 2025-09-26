using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeComponentItemUiStateService
    {
        RecipeComponentUIState GetOrCreate(RecipeComponentItemViewModel vm);
    }
}