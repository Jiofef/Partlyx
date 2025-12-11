using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.ServiceInterfaces;
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
        private readonly IRecipeComponentService _service;
        private readonly IGlobalSelectedParts _selectedParts;
        private readonly IDialogService _dialogService;
        private readonly IEventBus _bus;

        public RecipeComponentServiceViewModel(ICommandServices cs, IRecipeComponentService service, IGlobalSelectedParts gsp, IDialogService ds, IEventBus bus)
        {
            _commands = cs;
            _service = service;
            _selectedParts = gsp;
            _dialogService = ds;
            _bus = bus;
        }

        [RelayCommand]
        public async Task CreateComponentAsync(RecipeViewModel parent)
        {
            var selected = await ShowComponentCreateMenuAsync();

            if (selected == null)
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

        public async Task CreateComponentsFromAsync(RecipeViewModel parent, IEnumerable<ResourceViewModel> resources)
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
            var targetRecipe = info.Target;
            var components = info.Parts;

            await MoveComponentsAsync(targetRecipe, components);
        }

        public async Task MoveComponentsAsync(RecipeViewModel targetRecipe, List<RecipeComponentViewModel> components)
        {
            foreach (var component in components)
            {
                if (component.LinkedParentRecipe?.Value == targetRecipe)
                    continue;

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
            var grandParentUid = component.LinkedParentRecipe!.Value?.LinkedParentResource?.Uid;
            var parentUid = component.LinkedParentRecipe!.Uid;

            if (grandParentUid == null)
                return;

            bool exists = await _service.IsComponentExists((Guid)grandParentUid, component.Uid);
            
            if (exists)
                await _commands.CreateSyncAndExcecuteAsync<DeleteRecipeComponentCommand>(grandParentUid!, parentUid, component.Uid);
        }

        [RelayCommand]
        public async Task SetSelectedRecipe(PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel?> info)
        {
            if (info == null || info.Part == null || info.Value == null) return;

            var targetComponent = info.Part;
            var valueRecipe = info.Value;

            await SetSelectedRecipe(targetComponent, valueRecipe);
        }

        public async Task SetSelectedRecipe(RecipeComponentViewModel targetComponent, RecipeViewModel valueRecipe)
        {
            if (targetComponent.LinkedSelectedRecipe?.Value == valueRecipe)
                return;

            var grandParentUid = targetComponent.LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;

            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentSelectedRecipe>(grandParentUid, targetComponent.Uid, valueRecipe?.Uid!);
        }

        [RelayCommand]
        public async Task SetQuantityAsync(PartSetValueInfo<RecipeComponentViewModel, double> info)
        {
            if (info == null || info.Part == null) return;

            var targetComponent = info.Part;
            var value = info.Value;

            await SetQuantityAsync(targetComponent, value);
        }

        public async Task<ISelectedParts?> ShowComponentCreateMenuAsync()
        {
            var result = await _dialogService.ShowDialogAsync<ComponentCreateViewModel>();
            if (result is not ISelectedParts selected || !selected.IsResourcesSelected)
                return null;

            return selected;
        }
        public async Task SetQuantityAsync(RecipeComponentViewModel targetComponent, double value)
        {
            if (targetComponent.Quantity == value)
                return;

            var grandParentUid = targetComponent.LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;
            var uid = targetComponent.Uid;
            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentQuantityCommand>(grandParentUid, uid, value);
        }
    }
}
