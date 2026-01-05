using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Core.Contracts;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceImplementations;
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
        private readonly IGlobalFocusedElementContainer _focusedPart;

        public RecipeServiceViewModel(ICommandServices cs, ILocalizationService loc, IGlobalSelectedParts gsp, IGlobalFocusedElementContainer gfp, IEventBus bus, IRecipeService service,
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
        public async Task<Guid> CreateRecipeAsync(ResourceViewModel? parentResource = null, bool executeInLastComplex = false)
        {
            string recipeName = _loc["Recipe"];

            var parentResourceDto = parentResource?.ToDto();
            if (parentResourceDto != null)
                recipeName = parentResourceDto.Name;

            RecipeCreatingOptions options = new(recipeName, true, parentResourceDto);

            var createRecipeCommand = _commands.Factory.Create<CreateRecipeCommand>(options);
            // It must be executed on a single thread so that recipients respond to events immediately after they are sent
            await Task.Run(async () =>
            {
                if (executeInLastComplex)
                    await _commands.Dispatcher.ExcecuteInLastComplexAsync(createRecipeCommand);
                else
                    await _commands.Dispatcher.ExcecuteAsync(createRecipeCommand);

                // If we create a recipe for a resource, we want it to be immediately present in its components as an output
                if (parentResourceDto != null)
                {
                    var parentRecipeUid = createRecipeCommand.RecipeUid;
                    var componentAmount = _settings.DefaultRecipeOutputAmount;
                    var componentCreatingOptions = new RecipeComponentCreatingOptions(componentAmount, true, true);
                    await _commands.CreateAndExcecuteInLastComplexAsync<CreateRecipeComponentCommand>(parentRecipeUid, componentCreatingOptions);
                }
            });


            var recipeUid = createRecipeCommand.RecipeUid;
            _bus.Publish(new RecipeCreatingCompletedVMEvent(recipeUid));

            return recipeUid;
        }
        public async Task RenameRecipe(RecipeViewModel targetRecipe, string newName, bool executeInLastComplex = false)
        {
            if (targetRecipe.Name == newName)
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetRecipeNameCommand>(
                executeInLastComplex,
                targetRecipe.Uid, newName);
        }
        public async Task RemoveAsync(RecipeViewModel recipe, bool executeInLastComplex = false)
        {
            bool exists = await _service.IsRecipeExists(recipe.Uid);

            if (exists)
            {
                _bus.Publish(new RecipeDeletingStartedEvent(recipe.Uid,
                    new HashSet<object>() { recipe.Uid }));
                await _commands.CreateAndExcecuteInLastComplexAsyncIf<DeleteRecipeCommand>(
                    executeInLastComplex,
                    recipe.Uid);
            }
        }

        public async Task<Guid> Duplicate(RecipeViewModel recipe, bool executeInLastComplex = false)
        {
            var command = await _commands.CreateAndExcecuteInLastComplexAsyncIf<DuplicateRecipeCommand>(
                executeInLastComplex,
                recipe.Uid);

            var recipeUid = command.DuplicateUid;
            _bus.Publish(new RecipeCreatingCompletedVMEvent(recipeUid));

            return recipeUid;
        }
        public async Task SetIcon(RecipeViewModel targetRecipe, IconViewModel newIcon, bool executeInLastComplex = false)
        {
            var oldIcon = targetRecipe.Icon;

            if (oldIcon.IsIdentical(newIcon))
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetRecipeIconCommand>(
                executeInLastComplex,
                targetRecipe.Uid, newIcon.ToDto());
        }
        public async Task SetIsReversible(RecipeViewModel targetRecipe, bool isReversible, bool executeInLastComplex = false)
        {
            if (targetRecipe.IsReversible == isReversible)
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetRecipeIsReversibleCommand>(
                executeInLastComplex,
                targetRecipe.Uid, isReversible);
        }
    }
}
