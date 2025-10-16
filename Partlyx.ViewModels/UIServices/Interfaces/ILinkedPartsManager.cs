using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface ILinkedPartsManager
    {
        GuidLinkedPart<RecipeComponentViewModel> CreateAndRegisterLinkedRecipeComponentVM(Guid uid);
        GuidLinkedPart<RecipeViewModel> CreateAndRegisterLinkedRecipeVM(Guid uid);
        GuidLinkedPart<ResourceViewModel> CreateAndRegisterLinkedResourceVM(Guid uid);
        void Register(GuidLinkedPart<RecipeComponentViewModel> linkedPart);
        void Register(GuidLinkedPart<RecipeViewModel> linkedPart);
        void Register(GuidLinkedPart<ResourceViewModel> linkedPart);
    }
}