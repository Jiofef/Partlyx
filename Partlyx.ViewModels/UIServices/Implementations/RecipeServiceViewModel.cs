using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Contracts;
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
        private readonly ILocalizationService _loc;

        private readonly IGlobalSelectedParts _selectedParts;
        private readonly IGlobalFocusedPart _focusedPart;

        public RecipeServiceViewModel(ICommandServices cs, ILocalizationService loc, IGlobalSelectedParts gsp, IGlobalFocusedPart gfp)
        {
            _commands = cs;
            _loc = loc;
            _selectedParts = gsp;
            _focusedPart = gfp;
        }

        [RelayCommand]
        public async Task CreateRecipeAsync(ResourceViewModel parent)
        {
            int siblingsAmount = parent.Recipes.Count;
            var recipeName = siblingsAmount == 0
                            ? _loc["Recipe"]
                            : _loc.Get("Recipe_N", siblingsAmount + 1);

            await _commands.CreateSyncAndExcecuteAsync<CreateRecipeCommand>(parent.Uid, recipeName);
        }

        [RelayCommand]
        public void StartRenamingFocused()
        {
            var focused = _focusedPart.FocusedPart;
            if (focused is not RecipeViewModel recipe) return;

            recipe.UiItem.IsRenaming = true;
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
        public async Task SetCraftableAmount(PartSetValueInfo<RecipeViewModel, double> info)
        {
            var recipe = info.Part;
            var amount = info.Value;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeCraftAmountCommand>(recipe.LinkedParentResource!.Uid, recipe.Uid, amount);
        }

        [RelayCommand]
        public async Task RemoveAsync(RecipeViewModel recipe)
        {
            await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeCommand>(recipe.LinkedParentResource!.Uid, recipe.Uid);
        }
    }
}
