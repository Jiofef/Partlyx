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
        bool TryGet(Guid? itemUid, out IVMPart? part);

        /// <summary>
        /// Await appearance of an item with the specified Uid. If item already exists - completes immediately.
        /// If item does not exist yet - returns a task that completes when Register(...) is called for that Uid,
        /// or when timeout elapses (defaults to 5 seconds) which yields null.
        /// </summary>
        public Task<IVMPart?> TryGetAsync(Guid? itemUid, TimeSpan? timeout = null);
    }
}