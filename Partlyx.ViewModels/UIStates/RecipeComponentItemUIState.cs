using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Implementations;
using SQLitePCL;
namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentItemUIState : PartItemUIState
    {
        private readonly IEventBus _bus;
        private readonly PartsServiceViewModel _services;

        public RecipeComponentViewModel AttachedComponent { get; }
        public override IVMPart AttachedPart { get => AttachedComponent; }

        public override IFocusedElementContainer GlobalFocusedContainer => AttachedComponent.GlobalNavigations.FocusedElementContainer;
        public override IFocusable AttachedFocusable => AttachedComponent;

        public RecipeComponentItemUIState(RecipeComponentViewModel vm, PartsServiceViewModel svm, IEventBus bus, IGlobalFocusedElementContainer gfc)
        {
            _bus = bus;
            _services = svm;

            AttachedComponent = vm;

            var expandAllRecipeComponentItemsSubscription = bus.Subscribe<SetAllTheRecipeComponentItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Disposables.Add(expandAllRecipeComponentItemsSubscription);
        }

        [RelayCommand]
        public void FindResourceInTree()
        {
            var resource = AttachedComponent.LinkedResource?.Value;
            if (resource == null) return;

            resource.UiItem.FindInTree();
        }
        [RelayCommand]
        public override void FindInTree()
        {
            var resource = AttachedComponent.LinkedResource?.Value;
            if (resource == null) return;

            var query = resource.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Component);
            _bus.Publish(ev);
            IsSelected = true;
        }
        [RelayCommand]
        public async Task SetQuantityAsync(double value)
        {
            await _services.ComponentService.SetQuantityAsync(AttachedComponent, value);
        }
    }
}
