using Partlyx.ViewModels.PartsViewModels;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeItemUiStateService
    {
        RecipeItemUIState GetOrCreate(RecipeItemViewModel vm);
    }
}