
namespace Partlyx.ViewModels.PartsViewModels
{
    public interface IVMPartsStore
    {
        Dictionary<Guid, ResourceItemViewModel> Resources { get; }
        Dictionary<Guid, RecipeItemViewModel> Recipes { get; }
        Dictionary<Guid, RecipeComponentItemViewModel> RecipeComponents { get; }

        void Register(ResourceItemViewModel resource);
        void Register(RecipeItemViewModel recipe);
        void Register(RecipeComponentItemViewModel component);
    }
}