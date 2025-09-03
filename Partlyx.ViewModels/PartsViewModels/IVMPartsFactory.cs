using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.PartsViewModels
{
    public interface IVMPartsFactory
    {
        RecipeComponentItemViewModel CreateRecipeComponentVM(RecipeComponentDto dto);
        RecipeItemViewModel CreateRecipeVM(RecipeDto dto);
        ResourceItemViewModel CreateResourceVM(ResourceDto dto);
    }
}