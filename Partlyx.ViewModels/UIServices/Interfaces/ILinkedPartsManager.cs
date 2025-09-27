using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface ILinkedPartsManager
    {
        GuidLinkedPart<RecipeComponentItemViewModel> CreateAndRegisterLinkedRecipeComponentVM(Guid uid);
        GuidLinkedPart<RecipeItemViewModel> CreateAndRegisterLinkedRecipeVM(Guid uid);
        GuidLinkedPart<ResourceItemViewModel> CreateAndRegisterLinkedResourceVM(Guid uid);
        void Register(GuidLinkedPart<RecipeComponentItemViewModel> linkedPart);
        void Register(GuidLinkedPart<RecipeItemViewModel> linkedPart);
        void Register(GuidLinkedPart<ResourceItemViewModel> linkedPart);
    }
}