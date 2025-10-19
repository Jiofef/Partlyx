using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Contracts;
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

        public ResourceServiceViewModel(ICommandServices cs, IResourceService rs, IGlobalSelectedParts gsp, ILocalizationService loc) 
        {
            _commands = cs;
            _resourceService = rs;
            _selectedParts = gsp;
            _loc = loc;
        }

        [RelayCommand]
        public async Task CreateResourceAsync()
        {
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
                    var resourseUid = createResourceCommand.ResourceUid;

                    // Default recipe creating
                    var defaultRecipeCreateCommand = _commands.Factory.Create<CreateRecipeCommand>(resourseUid);
                    await complexDispatcher.ExcecuteAsync(defaultRecipeCreateCommand);
                });
            });
        }

        [RelayCommand]
        public void StartRenamingSelected()
        {
            var resourceVM = _selectedParts.GetSingleResourceOrNull();
            if (resourceVM == null) return;

            resourceVM.UiItem.IsRenaming = true;
        }

        [RelayCommand]
        public async Task RemoveAsync(ResourceViewModel resource)
        {
            await _commands.CreateSyncAndExcecuteAsync<DeleteResourceCommand>(resource.Uid);
        }

        [RelayCommand]
        public async Task RenameResource(PartSetValueInfo<ResourceViewModel, string> info)
        {
            string newName = info.Value;
            var resource = info.Part;

            await _commands.CreateAsyncEndExcecuteAsync<SetNameToResourceCommand>(resource.Uid, newName);
        }

        [RelayCommand]
        public async Task SetDefaultRecipe(PartSetValueInfo<ResourceViewModel, RecipeViewModel> info)
        {
            if (info == null || info.Part == null || info.Value == null) return;

            var resource = info.Part;
            var recipe = info.Value;

            await _commands.CreateAsyncEndExcecuteAsync<SetDefaultRecipeToResourceCommand>(resource.Uid, recipe.Uid);
        }
    }
}
