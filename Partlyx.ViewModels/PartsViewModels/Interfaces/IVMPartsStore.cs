using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPartsStore
    {
        IReadOnlyDictionary<Guid, ResourceItemViewModel> Resources { get; }
        IReadOnlyDictionary<Guid, RecipeItemViewModel> Recipes { get; }
        IReadOnlyDictionary<Guid, RecipeComponentItemViewModel> RecipeComponents { get; }

        void Register(ResourceItemViewModel resource);
        void Register(RecipeItemViewModel recipe);
        void Register(RecipeComponentItemViewModel component);
        void RemoveRecipe(Guid uid);
        void RemoveRecipeComponent(Guid uid);
        void RemoveResource(Guid uid);
        bool TryGet(Guid itemUid, out IVMPart? part);
    }
}