using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels
{
    public interface IDependencyTracker
    {
        void AddDependency(Guid resourceUid, RecipeComponentViewModel component);
        void RemoveDependency(Guid resourceUid, RecipeComponentViewModel component);
        IReadOnlyCollection<RecipeComponentViewModel> GetDependentComponents(Guid resourceUid);
    }
}
