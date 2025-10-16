using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeItemUiStateService
    {
        RecipeItemUIState GetOrCreateItemUi(RecipeViewModel vm);
        RecipeNodeUIState GetOrCreateNodeUi(RecipeViewModel vm);
    }
}