using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeComponentItemUiStateService
    {
        RecipeComponentItemUIState GetOrCreateItemUi(RecipeComponentViewModel vm);
        RecipeComponentNodeUIState GetOrCreateNodeUi(RecipeComponentViewModel vm);
    }
}