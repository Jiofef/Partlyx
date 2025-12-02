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
        public PartsServiceViewModel(ResourceServiceViewModel resService, RecipeServiceViewModel recService, RecipeComponentServiceViewModel comService,
            IGlobalFocusedPart focusedPart, IGlobalSelectedParts selectedParts) 
        {
            ResourceService = resService;
            RecipeService = recService;
            ComponentService = comService;

            _focusedPart = focusedPart;
            _selectedparts = selectedParts;
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
    }
}
