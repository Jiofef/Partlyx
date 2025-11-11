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

        public ResourceViewModel AttachedResource { get; }
        public override IVMPart AttachedPart { get => AttachedResource; }

        public ResourceItemUIState(ResourceViewModel vm, PartsServiceViewModel svm, IEventBus bus, IGlobalFocusedPart gfc)
        {
            _bus = bus;
            _services = svm;

            AttachedResource = vm;
            _unConfirmedName = vm.Name;

            var expandAllResourceItemsSubscription = bus.Subscribe<SetAllTheResourceItemsExpandedEvent>(
                ev => 
                SetExpanded(ev.expand));
            Subscriptions.Add(expandAllResourceItemsSubscription);
        }

        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        public override async Task HandleDrop(ISelectedParts droppedParts)
        {
            var dropType = droppedParts.GetOnlyNotEmptyCollectionPartsTypeOrNull();
            if (dropType == null) return;

            else if (dropType == PartTypeEnumVM.Recipe)
            {
                var recipes = droppedParts.Recipes.ToList();
                await _services.RecipeService.MoveRecipesAsync(AttachedResource, recipes);
            }
        }

        [RelayCommand]
        public async Task CommitNameChangeAsync()
        {
            if (!IsRenaming) return;

            var args = new PartSetValueInfo<ResourceViewModel, string>(AttachedResource, UnConfirmedName);
            await _services.ResourceService.RenameResource(args);

            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = AttachedResource.Name;
            IsRenaming = false;
        }

        [RelayCommand]
        public void StartRenaming()
            => IsRenaming = true;

        [RelayCommand]
        public void ExpandBranch()
        {
            IsExpanded = true;

            foreach (var child in AttachedResource.Recipes)
                child.UiItem.ExpandBranch();
        }

        [RelayCommand]
        public void CollapseBranch()
        {
            IsExpanded = false;

            foreach (var child in AttachedResource.Recipes)
                child.UiItem.CollapseBranch();
        }

        [RelayCommand]
        public void FindInTree()
        {
            var query = AttachedResource.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Resource);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void ToggleGlobalFocus()
        {
            var globalFocusedPart = AttachedResource.GlobalNavigations.FocusedPart;
            ToggleFocused(globalFocusedPart);
        }

        [RelayCommand]
        public void ToggleLocalFocus(IIsolatedFocusedPart target)
        {
            ToggleFocused(target);
        }
    }
}
