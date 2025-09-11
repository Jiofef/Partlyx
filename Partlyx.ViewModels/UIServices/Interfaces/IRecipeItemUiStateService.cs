namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeItemUiStateService
    {
        RecipeItemUIState GetOrCreate(Guid id);
    }
}