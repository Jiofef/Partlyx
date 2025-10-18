using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
namespace Partlyx.ViewModels.UIStates
{
    public partial class RecipeComponentItemUIState : PartItemUIState
    {
        private readonly IEventBus _bus;
        private readonly PartsServiceViewModel _services;

        private readonly RecipeComponentViewModel _componentVM;

        public RecipeComponentItemUIState(RecipeComponentViewModel vm, PartsServiceViewModel svm, IEventBus bus)
        {
            _bus = bus;
            _services = svm;

            _componentVM = vm;

            var expandAllPartItemsSubscription = bus.Subscribe<SetAllThePartItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Subscriptions.Add(expandAllPartItemsSubscription);
            var expandAllRecipeComponentItemsSubscription = bus.Subscribe<SetAllTheRecipeComponentItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Subscriptions.Add(expandAllRecipeComponentItemsSubscription);
        }

        [RelayCommand]
        public void FindResourceInTree()
        {
            var resource = _componentVM.LinkedResource?.Value;
            if (resource == null) return;

            resource.UiItem.FindInTree();
        }
    }
}
