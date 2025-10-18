using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels
{
    public partial class ResourceItemUIState : PartItemUIState
    {
        private readonly IEventBus _bus;
        private readonly PartsServiceViewModel _services;
        private readonly ResourceViewModel _resourceVM;

        public ResourceItemUIState(ResourceViewModel vm, PartsServiceViewModel svm, IEventBus bus)
        {
            _bus = bus;
            _services = svm;

            _resourceVM = vm;
            _unConfirmedName = vm.Name;

            var expandAllPartItemsSubscription = bus.Subscribe<SetAllThePartItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Subscriptions.Add(expandAllPartItemsSubscription);
            var expandAllResourceItemsSubscription = bus.Subscribe<SetAllTheResourceItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Subscriptions.Add(expandAllResourceItemsSubscription);
        }

        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        [RelayCommand]
        public async Task CommitNameChangeAsync()
        {
            if (!IsRenaming) return;

            var args = new PartSetValueInfo<ResourceViewModel, string>(_resourceVM, UnConfirmedName);
            await _services.ResourceService.RenameResource(args);

            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = _resourceVM.Name;
            IsRenaming = false;
        }

        [RelayCommand]
        public void StartRenaming()
            => IsRenaming = true;

        [RelayCommand]
        public void ExpandBranch()
        {
            IsExpanded = true;

            foreach (var child in _resourceVM.Recipes)
                child.UiItem.ExpandBranch();
        }

        [RelayCommand]
        public void CollapseBranch()
        {
            IsExpanded = false;

            foreach (var child in _resourceVM.Recipes)
                child.UiItem.CollapseBranch();
        }

        [RelayCommand]
        public void FindInTree()
        {
            var query = _resourceVM.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Resource);
            _bus.Publish(ev);
        }
    }
}
