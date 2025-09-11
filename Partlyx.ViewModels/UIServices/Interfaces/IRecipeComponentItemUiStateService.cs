namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IRecipeComponentItemUiStateService
    {
        RecipeComponentUIState GetOrCreate(Guid id);
    }
}