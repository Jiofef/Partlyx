using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;
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
        private readonly ApplicationSettingsProviderViewModel _settings;

        public RecipeComponentServiceViewModel(ICommandServices cs, IRecipeComponentService service, IGlobalSelectedParts gsp, IDialogService ds, IEventBus bus,
            ApplicationSettingsProviderViewModel settings)
        {
            _commands = cs;
            _service = service;
            _selectedParts = gsp;
            _dialogService = ds;
            _bus = bus;
            _settings = settings;
        }

        public async Task CreateInputAsync(RecipeViewModel parent)
            => await CreateComponentAsync(parent, false);
        public async Task CreateOutputAsync(RecipeViewModel parent)
            => await CreateComponentAsync(parent, true);

        public async Task CreateComponentAsync(RecipeViewModel parent, bool isOutput = false, bool executeInLastComplex = false)
        {
            var selected = await ShowComponentCreateMenuAsync();

            if (selected == null)
                return;

            var selectedResources = selected.Resources.ToList();
            await CreateComponentsFromAsync(parent, selectedResources, isOutput, executeInLastComplex);
        }

        public async Task CreateComponentsFromAsync(RecipeViewModel parent, ICollection<ResourceViewModel> resources, bool isOutput = false, bool executeInLastComplex = false)
        {
            var componentQuantity = isOutput ? _settings.DefaultRecipeOutputAmount : _settings.DefaultRecipeInputAmount;

            var resourceAmountPairs = resources
                .Select(r => new ResourceAmountPairViewModel(r, componentQuantity))
                .ToList();

            await CreateComponentsFromAsync(parent, resourceAmountPairs, isOutput, executeInLastComplex);
        }

        public async Task CreateComponentsFromAsync(RecipeViewModel parent, ICollection<ResourceAmountPairViewModel> resourceAmountPairs, bool isOutput = false, bool executeInLastComplex = false)
        {
            var parentRecipeUid = parent!.Uid;
            foreach (var pair in resourceAmountPairs)
            {
                var resource = pair.Resource;
                var componentQuantity = pair.Amount;
                var componentResUid = resource.Uid;

                var options = new RecipeComponentCreatingOptions(componentQuantity, true, isOutput);

                var command = _commands.Factory.Create<CreateRecipeComponentCommand>(parentRecipeUid, componentResUid, options);

                // It must be executed on a single thread so that recipients respond to events immediately after they are sent
                await Task.Run(async () =>
                {
                    if (executeInLastComplex)
                        await _commands.Dispatcher.ExcecuteInLastComplexAsync(command);
                    else
                        await _commands.Dispatcher.ExcecuteAsync(command);

                        var componentUid = command.RecipeComponentUid;
                    _bus.Publish(new RecipeComponentCreatingCompletedVMEvent(componentUid));
                });
            }
        }

        public async Task MoveComponentsAsync(RecipeViewModel targetRecipe, List<RecipeComponentViewModel> components, bool executeInLastComplex = false)
        {
            foreach (var component in components)
            {
                if (component.LinkedParentRecipe?.Value == targetRecipe)
                    continue;

                var previousParentUid = component.LinkedParentRecipe!.Uid;
                var newParentUid = targetRecipe.Uid;

                await _commands.CreateAndExcecuteInLastComplexAsyncIf<MoveRecipeComponentCommand>(
                    executeInLastComplex,
                    previousParentUid, newParentUid, component.Uid);
            }

            var componentUids = components.Select(c => c.Uid).ToArray();
            _bus.Publish(new RecipeComponentsMovingCompletedVMEvent(componentUids));
        }

        public async Task RemoveAsync(RecipeComponentViewModel component, bool executeInLastComplex = false)
        {
            var parentUid = component.LinkedParentRecipe!.Uid;

            bool exists = await _service.IsComponentExists(component.Uid);
            
            if (exists)
            {
                await Task.Run(async () =>
                {
                    _bus.Publish(new RecipeComponentDeletingStartedEvent(component.Uid, parentUid, 
                        new HashSet<object>() { component.Uid, parentUid }));
                    await Task.Run(async () => await _commands.CreateAndExcecuteInLastComplexAsyncIf<DeleteRecipeComponentCommand>(
                        executeInLastComplex,
                        component.Uid));
                });
            }
        }
        public async Task<Guid> Duplicate(RecipeComponentViewModel component, bool executeInLastComplex = false)
        {
            var command = await _commands.CreateAndExcecuteInLastComplexAsyncIf<DuplicateRecipeComponentCommand>(
                executeInLastComplex,
                component.Uid);

            var componentUid = command.DuplicateUid;
            _bus.Publish(new RecipeComponentCreatingCompletedVMEvent(componentUid));

            return componentUid;
        }

        public async Task MergeSameComponentsAsync(RecipeViewModel recipe, bool executeInLastComplex = false)
        {
            // Merging inputs
            var inputs = recipe.Inputs;
            var nonUniqueInputs = inputs.GetWithoutUniqueComponents();
            var mergedInputs = nonUniqueInputs.GetMerged();

            await Task.Run(async () =>
            {
                foreach (var component in nonUniqueInputs)
                    await RemoveAsync(component);

                await CreateComponentsFromAsync(recipe, mergedInputs, false, executeInLastComplex);
            });

            // Merging outputs
            var outputs = recipe.Outputs;
            var nonUniqueOutputs = outputs.GetWithoutUniqueComponents();
            var mergedOutputs = nonUniqueOutputs.GetMerged();

            await Task.Run(async () =>
            {
                foreach (var component in nonUniqueOutputs)
                    await RemoveAsync(component);

                await CreateComponentsFromAsync(recipe, mergedOutputs, true, executeInLastComplex);
            });
        }

        public async Task SetSelectedRecipe(RecipeComponentViewModel targetComponent, RecipeViewModel? valueRecipe, bool executeInLastComplex = false)
        {
            if (targetComponent.LinkedSelectedRecipe?.Value == valueRecipe)
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetRecipeComponentSelectedRecipe>(
                executeInLastComplex,
                targetComponent.Uid, valueRecipe?.Uid!);
        }

        public async Task<ISelectedParts?> ShowComponentCreateMenuAsync()
        {
            var result = await _dialogService.ShowDialogAsync<ComponentCreateViewModel>();
            if (result is not ISelectedParts selected || !selected.IsResourcesSelected)
                return null;

            return selected;
        }
        public async Task SetQuantityAsync(RecipeComponentViewModel targetComponent, double value, bool executeInLastComplex = false)
        {
            if (targetComponent.Quantity == value)
                return;

            await _commands.CreateAndExcecuteInLastComplexAsyncIf<SetRecipeComponentQuantityCommand>(
                executeInLastComplex,
                targetComponent.Uid, value);
        }
    }
}
