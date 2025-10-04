using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class RecipeServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IGlobalSelectedParts _selectedParts;

        public RecipeServiceViewModel(ICommandServices cs, IGlobalSelectedParts gsp)
        {
            _commands = cs;
            _selectedParts = gsp;
        }

        [RelayCommand]
        private async Task CreateRecipeAsync()
        {
            var parent = _selectedParts.GetSingleResourceOrNull();
            if (parent == null)
                throw new InvalidOperationException("Create command shouldn't be called when created part's parent isn't selected or is multiselected");

            await _commands.CreateSyncAndExcecuteAsync<CreateRecipeCommand>(parent.Uid);
        }

        [RelayCommand]
        private void StartRenamingSelected()
        {
            var recipeVM = _selectedParts.GetSingleRecipeOrNull();
            if (recipeVM == null) return;

            recipeVM.Ui.IsRenaming = true;
        }

        [RelayCommand]
        private async Task MoveComponentsAsync(PartsTargetInteractionInfo<RecipeComponentItemViewModel, RecipeItemViewModel> info)
        {
            var components = info.Parts;
            var targetRecipe = info.Target;

            foreach (var component in components)
            {
                var previousGrandParentUid = component.LinkedParentRecipe!.Value!.LinkedParentResource!.Value!.Uid;
                var newGrandParentUid = targetRecipe.LinkedParentResource!.Value!.Uid;
                var previousParentUid = component.LinkedParentRecipe.Uid;
                var newParentUid = targetRecipe.Uid;

                await _commands.CreateSyncAndExcecuteAsync<MoveRecipeComponentCommand>(previousGrandParentUid, newGrandParentUid, previousParentUid, newParentUid, component.Uid);
            }
        }
    }
}
