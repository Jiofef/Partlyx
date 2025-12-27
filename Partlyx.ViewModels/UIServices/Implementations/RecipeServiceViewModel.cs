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
        public async Task<Guid> CreateRecipeAsync()
        {
            int recipesCount = (await _service.GetAllTheRecipesAsync()).Count;
            var recipeName = recipesCount == 0
                            ? _loc["Recipe"]
                            : _loc.Get("Recipe_N", recipesCount + 1);

            var command = _commands.Factory.Create<CreateRecipeCommand>(recipeName);

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

            await RenameRecipe(targetRecipe, newName);
        }
        public async Task RenameRecipe(RecipeViewModel targetRecipe, string newName)
        {
            if (targetRecipe.Name == newName)
                return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeNameCommand>(targetRecipe.Uid, newName);
        }

        [RelayCommand]
        public async Task RemoveAsync(RecipeViewModel recipe)
        {
            bool exists = await _service.IsRecipeExists(recipe.Uid);

            if (exists)
            {
                _bus.Publish(new RecipeDeletingStartedEvent(recipe.Uid,
                    new HashSet<object>() { recipe.Uid }));
                await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeCommand>(recipe.Uid);
            }
        }

        public async Task<Guid> Duplicate(RecipeViewModel recipe)
        {
            var command = await _commands.CreateSyncAndExcecuteAsync<DuplicateRecipeCommand>(recipe.Uid);

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

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeIconCommand>(targetRecipe.Uid, newIcon.ToDto());
        }

        [RelayCommand]
        public async Task SetIsReversible(PartSetValueInfo<RecipeViewModel, bool> info)
        {
            if (info == null || info.Part == null) return;

            var targetRecipe = info.Part;
            var value = info.Value;

            await SetIsReversible(targetRecipe, value);
        }
        public async Task SetIsReversible(RecipeViewModel targetRecipe, bool isReversible)
        {
            if (targetRecipe.IsReversible == isReversible)
                return;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeIsReversibleCommand>(targetRecipe.Uid, isReversible);
        }
    }
}
