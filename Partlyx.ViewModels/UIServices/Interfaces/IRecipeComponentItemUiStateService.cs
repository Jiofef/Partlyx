using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeComponentItemUiStateService
    {
        RecipeComponentUIState GetOrCreateItemUi(RecipeComponentItemViewModel vm);
        RecipeComponentNodeUIState GetOrCreateNodeUi(RecipeComponentItemViewModel vm);
    }
}