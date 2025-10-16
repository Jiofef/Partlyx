using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPartsFactory
    {
        RecipeComponentViewModel GetOrCreateRecipeComponentVM(RecipeComponentDto dto);
        RecipeViewModel GetOrCreateRecipeVM(RecipeDto dto);
        ResourceViewModel GetOrCreateResourceVM(ResourceDto dto);
    }
}