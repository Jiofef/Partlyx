using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface ISelectedParts
    {
        ReadOnlyObservableCollection<ResourceViewModel> Resources { get; }
        ReadOnlyObservableCollection<RecipeViewModel> Recipes { get; }
        ReadOnlyObservableCollection<RecipeComponentViewModel> Components { get; }
        bool IsSingleResourceSelected { get; }
        bool IsSingleRecipeSelected { get; }
        bool IsSingleComponentSelected { get; }
        bool IsPartsSelected { get; }

        RecipeComponentViewModel? SingleComponentOrNull { get; }
        RecipeViewModel? SingleRecipeOrNull { get; }
        ResourceViewModel? SingleResourceOrNull { get; }

        bool IsResourcesSelected { get; }
        bool IsRecipesSelected { get; }
        bool IsComponentsSelected { get; }

        void AddComponentToSelected(RecipeComponentViewModel component);
        void AddPartToSelected(IVMPart part);
        void AddRecipeToSelected(RecipeViewModel recipe);
        void AddResourceToSelected(ResourceViewModel resource);

        void ClearSelectedComponents();
        void ClearSelectedRecipes();
        void ClearSelectedResources();
        void ClearSelection();

        void RemoveResourceFromSelected(ResourceViewModel resource);
        void RemoveRecipeFromSelected(RecipeViewModel recipe);
        void RemoveComponentFromSelected(RecipeComponentViewModel component);
        RecipeComponentViewModel? GetSingleComponentOrNull();
        RecipeViewModel? GetSingleRecipeOrNull();
        ResourceViewModel? GetSingleResourceOrNull();

        void SelectSingleComponent(RecipeComponentViewModel component);
        void SelectSingleComponentAncestors(RecipeComponentViewModel component);
        void SelectSinglePart(IVMPart part);
        void SelectSingleRecipe(RecipeViewModel recipe);
        void SelectSingleRecipeAncestor(RecipeViewModel recipe);
        void SelectSingleResource(ResourceViewModel resource);
    }

    public interface IGlobalSelectedParts : ISelectedParts { }

    public interface IIsolatedSelectedParts : ISelectedParts { }
}
