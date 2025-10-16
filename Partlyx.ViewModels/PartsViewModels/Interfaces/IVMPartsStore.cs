using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPartsStore
    {
        IReadOnlyDictionary<Guid, ResourceViewModel> Resources { get; }
        IReadOnlyDictionary<Guid, RecipeViewModel> Recipes { get; }
        IReadOnlyDictionary<Guid, RecipeComponentViewModel> RecipeComponents { get; }

        void Register(ResourceViewModel resource);
        void Register(RecipeViewModel recipe);
        void Register(RecipeComponentViewModel component);
        void RemoveRecipe(Guid uid);
        void RemoveRecipeComponent(Guid uid);
        void RemoveResource(Guid uid);
        bool TryGet(Guid itemUid, out IVMPart? part);
    }
}