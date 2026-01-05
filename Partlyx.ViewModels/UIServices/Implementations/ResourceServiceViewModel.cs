using CommunityToolkit.Mvvm.Input;
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
    public partial class ResourceServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IResourceService _resourceService;
        private readonly IGlobalSelectedParts _selectedParts;
        private readonly ILocalizationService _loc;
        private readonly IEventBus _bus;
        private readonly ApplicationSettingsProviderViewModel _settings;

        public ResourceServiceViewModel(ICommandServices cs, IResourceService rs, IGlobalSelectedParts gsp, ILocalizationService loc, IEventBus bus, ApplicationSettingsProviderViewModel settings) 
        {
            _commands = cs;
            _resourceService = rs;
            _selectedParts = gsp;
            _loc = loc;
            _bus = bus;
            _settings = settings;
        }
        public async Task<Guid> CreateResourceAsync(bool executeInLastComplex = false)
        {
            Guid resourceUid = Guid.Empty;
            // It must be executed on a single thread so that recipients respond to events immediately after they are sent
            await Task.Run(async () =>
            {
                await _commands.Dispatcher.ExcecuteComplexAsync(async complexDispatcher =>
                {
                    // Resource creating
                    int resourcesCount = await _resourceService.GetResourcesCountAsync();

                    var resourceName = _loc["Resource"];

                    var createResourceCommand = _commands.Factory.Create<CreateResourceCommand>(resourceName);
                    await complexDispatcher.ExcecuteAsync(createResourceCommand);
                    resourceUid = createResourceCommand.ResourceUid;

                    if (_settings.CreateResourceWithRecipeByDefault)
                    {
                        var resourceDto = await _resourceService.GetResourceAsync(resourceUid);
                        if (resourceDto == null)
                            throw new InvalidOperationException("Created resource was not found in db");

                        RecipeCreatingOptions recipeCreatingOptions = new(resourceName, true, resourceDto);

                        var defaultRecipeCreateCommand = _commands.Factory.Create<CreateRecipeCommand>(_loc["Recipe"]);
                        await complexDispatcher.ExcecuteAsync(defaultRecipeCreateCommand);

                        var recipeUid = defaultRecipeCreateCommand.RecipeUid;
                        _bus.Publish(new RecipeCreatingCompletedVMEvent(recipeUid));

                        // If we create a recipe for a resource, we want it to be immediately present in its components as an output
                        var componentAmount = _settings.DefaultRecipeOutputAmount;
                        var componentCreatingOptions = new RecipeComponentCreatingOptions(componentAmount, true, true);
                        await _commands.CreateAndExcecuteInLastComplexAsync<CreateRecipeComponentCommand>(recipeUid, resourceUid, componentCreatingOptions);
                    }
                });
            });
            _bus.Publish(new ResourceCreatingCompletedVMEvent(resourceUid));

            return resourceUid;
        }
        public async Task RemoveAsync(ResourceViewModel resource, bool executeInLastComplex = false)
        {
            _bus.Publish(new ResourceDeletingStartedEvent(resource.Uid));
            await _commands.CreateAndExcecuteInLastComplexAsyncIf<DeleteResourceCommand>(
                executeInLastComplex,
                resource.Uid);
        }
        public async Task<Guid> Duplicate(ResourceViewModel resource, bool executeInLastComplex = false)
        {
            var command = await _commands.CreateAndExcecuteInLastComplexAsyncIf<DuplicateResourceCommand>(
                executeInLastComplex, 
                resource.Uid);

            var resourceUid = command.DuplicateUid;
            _bus.Publish(new ResourceCreatingCompletedVMEvent(resourceUid));

            return command.DuplicateUid;
        }

        public async Task RenameResource(ResourceViewModel resource, string newName, bool executeInLastComplex = false)
        {
            if (resource.Name == newName)
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetNameToResourceCommand>(
                executeInLastComplex,
                resource.Uid, newName);
        }
        public async Task SetDefaultRecipe(ResourceViewModel resource, RecipeViewModel recipe, bool executeInLastComplex = false)
        {
            if (resource.LinkedDefaultRecipe?.Value == recipe)
                return;

            if (!recipe.HasResourceInLinkedComponents(resource.Uid))
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetDefaultRecipeToResourceCommand>(
                executeInLastComplex,
                resource.Uid, recipe.Uid);
        }

        public async Task SetIcon(ResourceViewModel resource, IconViewModel newIcon, bool executeInLastComplex = false)
        {
            var oldIcon = resource.Icon;

            if (oldIcon.IsIdentical(newIcon))
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetResourceIconCommand>(
                executeInLastComplex,
                resource.Uid, newIcon.ToDto());
        }
    }
}
