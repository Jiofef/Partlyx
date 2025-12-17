using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Runtime.CompilerServices;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class PartsServiceViewModel
    {
        private readonly IGlobalFocusedPart _focusedPart;
        private readonly IGlobalSelectedParts _selectedparts;
        private readonly IVMPartsStore _store;
        public PartsServiceViewModel(ResourceServiceViewModel resService, RecipeServiceViewModel recService, RecipeComponentServiceViewModel comService,
            IGlobalFocusedPart focusedPart, IGlobalSelectedParts selectedParts, IVMPartsStore store) 
        {
            ResourceService = resService;
            RecipeService = recService;
            ComponentService = comService;

            _focusedPart = focusedPart;
            _selectedparts = selectedParts;
            _store = store;
        }

        public ResourceServiceViewModel ResourceService { get; }
        public RecipeServiceViewModel RecipeService { get; }
        public RecipeComponentServiceViewModel ComponentService { get; }

        [RelayCommand]
        public void TryRenamingFocused()
        {
            var focused = _focusedPart.FocusedPart;
            if (focused == null) return;

            if (focused is ResourceViewModel resource)
            {
                resource.UiItem.StartRenaming();
            }
            else if (focused is RecipeViewModel recipe)
            {
                recipe.UiItem.StartRenaming();
            }
        }

        [RelayCommand]
        public void TryRenamingSingleSelected()
        {
            var selectedPartsList = _selectedparts.GetAllTheParts();

            if (selectedPartsList.Count != 1) return;

            var singleSelected = selectedPartsList.Single();

            if (singleSelected is ResourceViewModel resource)
            {
                resource.UiItem.StartRenaming();
            }
            else if (singleSelected is RecipeViewModel recipe)
            {
                recipe.UiItem.StartRenaming();
            }
        }

        [RelayCommand]
        public void TryToggleSingleSelectedFocus()
        {
            var selectedPartsList = _selectedparts.GetAllTheParts();

            if (selectedPartsList.Count != 1) return;

            var singleSelected = selectedPartsList.Single();
            singleSelected.UiItem.ToggleGlobalFocus();
        }

        [RelayCommand]
        public async Task RemoveAsync(IVMPart part)
        {
            if (part is ResourceViewModel resource)
                await ResourceService.RemoveAsync(resource);
            else if (part is RecipeViewModel recipe)
                await RecipeService.RemoveAsync(recipe);
            else if (part is RecipeComponentViewModel component)
                await ComponentService.RemoveAsync(component);
        }

        [RelayCommand]
        public async Task Duplicate(IVMPart part)
        {
            if (part is ResourceViewModel resource)
                await ResourceService.Duplicate(resource);
            else if (part is RecipeViewModel recipe)
                await RecipeService.Duplicate(recipe);
            else if (part is RecipeComponentViewModel component)
                await ComponentService.Duplicate(component);
        }

        [RelayCommand]
        public async Task RemoveChildrenFrom(IVMPart part)
        {
            if (part is ResourceViewModel resource)
            {
                foreach (var recipe in resource.Recipes)
                    await RecipeService.RemoveAsync(recipe);
            }
            if (part is RecipeViewModel recipe1)
            {
                foreach (var component in recipe1.Components)
                    await ComponentService.RemoveAsync(component);
            }
        }

        [RelayCommand]
        public async Task CreateQuantifiedCloneAsync(RecipeViewModel recipe)
        {
            // It shouldn't execute on UI
            await Task.Run(async () =>
            {
                var quantifiedComponents = recipe.GetQuantifiedList();

                // Preparing a recipe copy for quantifying
                var recipeCloneUid = await RecipeService.Duplicate(recipe);
                var recipeClone = _store.Recipes.GetValueOrDefault(recipeCloneUid);
                if (recipeClone == null) return;

                await RemoveChildrenFrom(recipeClone);

                await ComponentService.CreateComponentsFromAsync(recipeClone, quantifiedComponents);
            });
        }
    }
}
