using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeItemUiStateService
    {
        RecipeItemUIState GetOrCreate(RecipeItemViewModel vm);
    }
}