using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
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
        public async Task CreateRecipeAsync(ResourceViewModel parent)
        {
            await _commands.CreateSyncAndExcecuteAsync<CreateRecipeCommand>(parent.Uid);
        }

        [RelayCommand]
        public void StartRenamingSelected()
        {
            var recipeVM = _selectedParts.GetSingleRecipeOrNull();
            if (recipeVM == null) return;

            recipeVM.UiItem.IsRenaming = true;
        }

        [RelayCommand]
        public async Task RenameRecipe(PartSetValueInfo<RecipeViewModel, string> info)
        {
            string newName = info.Value;
            var recipe = info.Part;
            if (recipe.LinkedParentResource == null) return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeNameCommand>(recipe.LinkedParentResource!.Uid, recipe.Uid, newName);
        }

        [RelayCommand]
        public async Task MoveRecipesAsync(PartsTargetInteractionInfo<RecipeViewModel, ResourceViewModel> info)
        {
            var recipes = info.Parts;
            var targetResource = info.Target;

            foreach (var recipe in recipes)
            {
                var previousParentUid = recipe.LinkedParentResource!.Uid;
                var newParentUid = targetResource.Uid;

                await _commands.CreateSyncAndExcecuteAsync<MoveRecipeCommand>(previousParentUid, newParentUid, recipe.Uid);
            }
        }

        [RelayCommand]
        public async Task RemoveAsync(RecipeViewModel recipe)
        {
            await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeCommand>(recipe.LinkedParentResource!.Uid, recipe.Uid);
        }
    }
}
