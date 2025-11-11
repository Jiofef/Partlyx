using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

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
            var selectedPartsList = new List<IVMPart>();
            selectedPartsList.AddRange(_selectedparts.Resources);
            selectedPartsList.AddRange(_selectedparts.Recipes);
            selectedPartsList.AddRange(_selectedparts.Components);

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
    }
}
