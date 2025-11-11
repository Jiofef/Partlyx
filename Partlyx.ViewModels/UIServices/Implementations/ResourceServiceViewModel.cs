using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Contracts;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class ResourceServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IResourceService _resourceService;
        private readonly IGlobalSelectedParts _selectedParts;
        private readonly ILocalizationService _loc;
        private readonly IEventBus _bus;

        public ResourceServiceViewModel(ICommandServices cs, IResourceService rs, IGlobalSelectedParts gsp, ILocalizationService loc, IEventBus bus) 
        {
            _commands = cs;
            _resourceService = rs;
            _selectedParts = gsp;
            _loc = loc;
            _bus = bus;
        }

        [RelayCommand]
        public async Task CreateResourceAsync()
        {
            Guid resourceUid = Guid.Empty;
            // It must be executed on a single thread so that recipients respond to events immediately after they are sent
            await Task.Run(async () =>
            {
                await _commands.Dispatcher.ExcecuteComplexAsync(async complexDispatcher =>
                {
                    // Resource creating
                    int resourcesCount = await _resourceService.GetResourcesCountAsync();

                    var resourceName = resourcesCount == 0
                            ? _loc["Resource"]
                            : _loc.Get("Resource_N", resourcesCount + 1);

                    var createResourceCommand = _commands.Factory.Create<CreateResourceCommand>(resourceName);
                    await complexDispatcher.ExcecuteAsync(createResourceCommand);
                    resourceUid = createResourceCommand.ResourceUid;

                    // Default recipe creating
                    var defaultRecipeCreateCommand = _commands.Factory.Create<CreateRecipeCommand>(resourceUid, _loc["Recipe"]);
                    await complexDispatcher.ExcecuteAsync(defaultRecipeCreateCommand);
                });
            });

            _bus.Publish(new ResourceCreatingCompletedVMEvent(resourceUid));
        }

        [RelayCommand]
        public async Task RemoveAsync(ResourceViewModel resource) =>
            await _commands.CreateSyncAndExcecuteAsync<DeleteResourceCommand>(resource.Uid);

        [RelayCommand]
        public async Task RenameResource(PartSetValueInfo<ResourceViewModel, string> info)
        {
            var resource = info.Part;
            string newName = info.Value;

            await RenameResource(resource, newName);
        }
        public async Task RenameResource(ResourceViewModel resource, string newName) =>
            await _commands.CreateAsyncEndExcecuteAsync<SetNameToResourceCommand>(resource.Uid, newName);

        [RelayCommand]
        public async Task SetDefaultRecipe(PartSetValueInfo<ResourceViewModel, RecipeViewModel> info)
        {
            if (info == null || info.Part == null || info.Value == null) return;

            var targetResource = info.Part;
            var valueRecipe = info.Value;

            await SetDefaultRecipe(targetResource, valueRecipe);
        }
        public async Task SetDefaultRecipe(ResourceViewModel resource, RecipeViewModel recipe) =>
            await _commands.CreateAsyncEndExcecuteAsync<SetDefaultRecipeToResourceCommand>(resource.Uid, recipe.Uid);
    }
}
