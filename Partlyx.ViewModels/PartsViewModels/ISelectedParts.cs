namespace Partlyx.ViewModels.PartsViewModels
{
    public interface ISelectedParts
    {
        RecipeComponentItemViewModel? Component { get; }
        RecipeItemViewModel? Recipe { get; }
        ResourceItemViewModel? Resource { get; }

        void SetComponent(RecipeComponentItemViewModel? component);
        void SetRecipe(RecipeItemViewModel? recipe);
        void SetResource(ResourceItemViewModel? resource);
    }
}