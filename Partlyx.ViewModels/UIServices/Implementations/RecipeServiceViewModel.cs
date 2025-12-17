using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Core.Contracts;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class RecipeServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly ILocalizationService _loc;
        private readonly IEventBus _bus;
        private readonly IRecipeService _service;
        private readonly ApplicationSettingsProviderViewModel _settings;

        private readonly IGlobalSelectedParts _selectedParts;
        private readonly IGlobalFocusedPart _focusedPart;

        public RecipeServiceViewModel(ICommandServices cs, ILocalizationService loc, IGlobalSelectedParts gsp, IGlobalFocusedPart gfp, IEventBus bus, IRecipeService service,
            ApplicationSettingsProviderViewModel settings)
        {
            _commands = cs;
            _loc = loc;
            _bus = bus;
            _service = service;
            _settings = settings;

            _selectedParts = gsp;
            _focusedPart = gfp;
        }

        [RelayCommand]
        public async Task<Guid> CreateRecipeAsync(ResourceViewModel parent)
        {
            int siblingsAmount = parent.Recipes.Count;
            var recipeName = siblingsAmount == 0
                            ? _loc["Recipe"]
                            : _loc.Get("Recipe_N", siblingsAmount + 1);
            var craftAmount = _settings.DefaultRecipeCraftAmount;

            var command = _commands.Factory.Create<CreateRecipeCommand>(parent.Uid, recipeName, craftAmount);

            // It must be executed on a single thread so that recipients respond to events immediately after they are sent
            await Task.Run(async () =>
            {
                await _commands.Dispatcher.ExcecuteAsync(command);
            });
                 

            var recipeUid = command.RecipeUid;
            _bus.Publish(new RecipeCreatingCompletedVMEvent(recipeUid));

            return recipeUid;
        }

        [RelayCommand]
        public async Task RenameRecipe(PartSetValueInfo<RecipeViewModel, string> info)
        {
            var targetRecipe = info.Part;
            string newName = info.Value;
            if (targetRecipe.LinkedParentResource == null) return;

            await RenameRecipe(targetRecipe, newName);
        }
        public async Task RenameRecipe(RecipeViewModel targetRecipe, string newName)
        {
            if (targetRecipe.Name == newName)
                return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeNameCommand>(targetRecipe.LinkedParentResource!.Uid, targetRecipe.Uid, newName);
        }

        [RelayCommand]
        public async Task MoveRecipesAsync(PartsTargetInteractionInfo<RecipeViewModel, ResourceViewModel> info)
        {
            var targetResource = info.Target;
            var recipes = info.Parts;

            await (MoveRecipesAsync(targetResource, recipes));
        }

        public async Task MoveRecipesAsync(ResourceViewModel targetResource, List<RecipeViewModel> recipes)
        {
            foreach (var recipe in recipes)
            {
                if (recipe.LinkedParentResource?.Value == targetResource) continue;

                var previousParentUid = recipe.LinkedParentResource!.Uid;
                var newParentUid = targetResource.Uid;

                await _commands.CreateSyncAndExcecuteAsync<MoveRecipeCommand>(previousParentUid, newParentUid, recipe.Uid);
            }

            var recipeUids = recipes.Select(r => r.Uid).ToArray();
            _bus.Publish(new RecipesMovingCompletedVMEvent(recipeUids));
        }

        [RelayCommand]
        public async Task SetCraftableAmount(PartSetValueInfo<RecipeViewModel, double> info)
        {
            var targetRecipe = info.Part;
            var amount = info.Value;

            await SetCraftableAmount(targetRecipe, amount);
        }
        public async Task SetCraftableAmount(RecipeViewModel targetRecipe, double amount)
        {
            if (targetRecipe.CraftAmount == amount) return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeCraftAmountCommand>(targetRecipe.LinkedParentResource!.Uid, targetRecipe.Uid, amount);
        }

        [RelayCommand]
        public async Task RemoveAsync(RecipeViewModel recipe)
        {
            bool exists = await _service.IsRecipeExists(recipe.LinkedParentResource!.Uid, recipe.Uid);

            if (exists)
            {
                _bus.Publish(new RecipeDeletingStartedEvent(recipe.Uid, recipe.LinkedParentResource!.Value!.Uid,
                    new HashSet<object>() { recipe.Uid, recipe.LinkedParentResource!.Value!.Uid }));
                await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeCommand>(recipe.LinkedParentResource!.Uid, recipe.Uid);
            }
        }

        public async Task<Guid> Duplicate(RecipeViewModel recipe)
        {
            var parentUid = recipe.LinkedParentResource!.Uid;
            var command = await _commands.CreateSyncAndExcecuteAsync<DuplicateRecipeCommand>(parentUid, recipe.Uid);

            var recipeUid = command.DuplicateUid;
            _bus.Publish(new RecipeCreatingCompletedVMEvent(recipeUid));

            return recipeUid;
        }

        [RelayCommand]
        public async Task SetIcon(PartSetValueInfo<RecipeViewModel, IconViewModel> info)
        {
            if (info == null || info.Part == null || info.Value == null) return;

            var targetRecipe = info.Part;
            var valueIcon = info.Value;

            await SetIcon(targetRecipe, valueIcon);
        }
        public async Task SetIcon(RecipeViewModel targetRecipe, IconViewModel newIcon)
        {
            var oldIcon = targetRecipe.Icon;

            if (oldIcon.IsIdentical(newIcon))
                return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeIconCommand>(targetRecipe.LinkedParentResource!.Uid, targetRecipe.Uid, newIcon.ToDto());
        }
    }
}
