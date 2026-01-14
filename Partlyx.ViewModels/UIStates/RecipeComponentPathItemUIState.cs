using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentPathItemUIState : FocusableItemUIState
    {
        private readonly PartsServiceViewModel _partsService;
        private readonly IVMPartsStore _store;
        public RecipeComponentPathItem PathItem { get; }

        public override IGlobalFocusedElementContainer GlobalFocusedContainer { get; }

        public override IFocusable AttachedFocusable => PathItem;

        public RecipeComponentPathItemUIState(RecipeComponentPathItem pathItem, IGlobalFocusedElementContainer focusedContainer, PartsServiceViewModel partsService, IVMPartsStore store)
        {
            PathItem = pathItem;
            GlobalFocusedContainer = focusedContainer;
            _partsService = partsService;
            _store = store;
        }

        [RelayCommand]
        public async Task MakeARecipe()
        {
            var inputs = PathItem.SavedInputSums.ToList();
            var outputs = PathItem.SavedOutputSums.ToList();

            var mainOutput = PathItem.Path.GetLast()?.Value;

            // Creating the base recipe
            var recipeUid = await _partsService.RecipeService.CreateRecipeAsync(mainOutput?.Resource, false);

            var parentRecipe = _store.Recipes.GetValueOrDefault(recipeUid);
            if (parentRecipe == null)
                return;

            // Creating the inputs
            await _partsService.ComponentService.CreateComponentsFromAsync(parentRecipe, inputs, false, true);

            // Creating the outputs
            await _partsService.ComponentService.CreateComponentsFromAsync(parentRecipe, outputs, true, true);
        }
    }
}
