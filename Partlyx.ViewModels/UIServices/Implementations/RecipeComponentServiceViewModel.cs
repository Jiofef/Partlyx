using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.ComponentModel;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class RecipeComponentServiceViewModel
    {
        private readonly ICommandServices _commands;
        private readonly IGlobalSelectedParts _selectedParts;
        private readonly IDialogService _dialogService;
        private readonly IEventBus _bus;

        public RecipeComponentServiceViewModel(ICommandServices cs, IGlobalSelectedParts gsp, IDialogService ds, IEventBus bus)
        {
            _commands = cs;
            _selectedParts = gsp;
            _dialogService = ds;
            _bus = bus;
        }

        [RelayCommand]
        public async Task CreateComponentAsync(RecipeViewModel parent)
        {
            var result = await _dialogService.ShowDialogAsync<ComponentCreateViewModel>();
            if (result is not ISelectedParts selected || !selected.IsResourcesSelected)
                return;

            var selectedResources = selected.Resources.ToList();
            await CreateComponentsFromAsync(parent, selectedResources);
        }

        [RelayCommand]
        public async Task CreateComponentsFromAsync(PartsTargetInteractionInfo<ResourceViewModel, RecipeViewModel> info)
        {
            var parent = info.Target;
            var resources = info.Parts;

            await CreateComponentsFromAsync(parent, resources);
        }

        public async Task CreateComponentsFromAsync(RecipeViewModel parent, List<ResourceViewModel> resources)
        {
            var grandParentResUid = parent.LinkedParentResource!.Uid;
            var parentRecipeUid = parent!.Uid;
            foreach (var resource in resources)
            {
                var componentResUid = resource.Uid;
                var command = _commands.Factory.Create<CreateRecipeComponentCommand>(grandParentResUid, parentRecipeUid, componentResUid);

                // It must be executed on a single thread so that recipients respond to events immediately after they are sent
                await Task.Run(async () =>
                {
                    await _commands.Dispatcher.ExcecuteAsync(command);
                });

                var componentUid = command.RecipeComponentUid;
                _bus.Publish(new RecipeComponentCreatingCompletedVMEvent(componentUid));
            }
        }

        [RelayCommand]
        public async Task MoveComponentsAsync(PartsTargetInteractionInfo<RecipeComponentViewModel, RecipeViewModel> info)
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

            var componentUids = components.Select(c => c.Uid).ToArray();
            _bus.Publish(new RecipeComponentsMovingCompletedVMEvent(componentUids));
        }

        [RelayCommand]
        public async Task RemoveAsync(RecipeComponentViewModel component)
        {
            var grandParentUid = component.LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;
            var parentUid = component.LinkedParentRecipe!.Uid;
            await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeComponentCommand>(grandParentUid, parentUid, component.Uid);
        }

        [RelayCommand]
        public async Task SetSelectedRecipe(PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?> info)
        {
            if (info == null || info.Part == null || info.Value == null) return;

            var component = info.Part;
            var targetRecipe = info.Value;

            var grandParentUid = component.LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentSelectedRecipe>(grandParentUid, component.Uid, targetRecipe?.Uid!);
        }

        [RelayCommand]
        public async Task SetQuantityAsync(PartSetValueInfo<RecipeComponentViewModel, double> info)
        {
            if (info == null || info.Part == null) return;

            var component = info.Part;
            var value = info.Value;

            var grandParentUid = component.LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;
            var uid = component.Uid;
            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentQuantityCommand>(grandParentUid, uid, value);
        }
    }
}
