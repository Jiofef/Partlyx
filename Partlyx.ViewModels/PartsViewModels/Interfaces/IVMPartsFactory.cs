using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPartsFactory
    {
        RecipeComponentItemViewModel GetOrCreateRecipeComponentVM(RecipeComponentDto dto);
        RecipeItemViewModel GetOrCreateRecipeVM(RecipeDto dto);
        ResourceItemViewModel GetOrCreateResourceVM(ResourceDto dto);
    }
}