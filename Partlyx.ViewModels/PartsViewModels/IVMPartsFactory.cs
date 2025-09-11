using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.PartsViewModels
{
    public interface IVMPartsFactory
    {
        RecipeComponentItemViewModel GetOrCreateRecipeComponentVM(RecipeComponentDto dto);
        RecipeItemViewModel GetOrCreateRecipeVM(RecipeDto dto);
        ResourceItemViewModel GetOrCreateResourceVM(ResourceDto dto);
    }
}