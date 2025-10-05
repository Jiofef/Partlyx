using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class RecipeComponentServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IGlobalSelectedParts _selectedParts;
        private readonly IDialogService _dialogService;

        public RecipeComponentServiceViewModel(ICommandServices cs, IGlobalSelectedParts gsp, IDialogService ds)
        {
            _commands = cs;
            _selectedParts = gsp;
            _dialogService = ds;
        }

        [RelayCommand]
        public async Task CreateComponentAsync(RecipeItemViewModel parent)
        {
            var result = await _dialogService.ShowDialogAsync<ComponentCreateViewModel>();
            if (result is not ISelectedParts selected || !selected.IsResourcesSelected)
                return;

            var selectedResources = selected.Resources.ToList();
            await CreateComponentsFromAsync(parent, selectedResources);
        }

        [RelayCommand]
        public async Task CreateComponentsFromAsync(PartsTargetInteractionInfo<ResourceItemViewModel, RecipeItemViewModel> info)
        {
            var parent = info.Target;
            var resources = info.Parts;

            await CreateComponentsFromAsync(parent, resources);
        }

        public async Task CreateComponentsFromAsync(RecipeItemViewModel parent, List<ResourceItemViewModel> resources)
        {
            var grandParentResUid = parent.LinkedParentResource!.Uid;
            var parentRecipeUid = parent!.Uid;
            foreach (var resource in resources)
            {
                var componentResUid = resource.Uid;
                var command = _commands.Factory.Create<CreateRecipeComponentCommand>(grandParentResUid, parentRecipeUid, componentResUid);
                await _commands.Dispatcher.ExcecuteAsync(command);
            }
        }

        [RelayCommand]
        public async Task MoveComponentsAsync(PartsTargetInteractionInfo<RecipeComponentItemViewModel, RecipeItemViewModel> info)
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
