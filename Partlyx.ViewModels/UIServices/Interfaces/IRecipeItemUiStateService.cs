using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeItemUiStateService
    {
        RecipeItemUIState GetOrCreateItemUi(RecipeItemViewModel vm);
        RecipeNodeUIState GetOrCreateNodeUi(RecipeItemViewModel vm);
    }
}