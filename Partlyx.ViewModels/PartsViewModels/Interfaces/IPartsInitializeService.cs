namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IPartsInitializeService
    {
        bool InitializationFinished { get; }
        bool IsRecipeComponentsLoaded { get; }
        bool IsRecipesLoaded { get; }
        bool IsResourcesLoaded { get; }

        void Dispose();
    }
}