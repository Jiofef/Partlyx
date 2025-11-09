using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIObjectViewModels;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIStates;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels
{
    public partial class RecipeItemUIState : PartItemUIState
    {
        private readonly IEventBus _bus;
        private readonly PartsServiceViewModel _services;
        public RecipeViewModel AttachedRecipe { get; }
        public override IVMPart AttachedPart { get => AttachedRecipe; }
        public IGlobalFocusedPart GlobalFocusedPart { get; }

        public RecipeItemUIState(RecipeViewModel vm, IEventBus bus, PartsServiceViewModel cvm, IGlobalFocusedPart gfc) 
        {
            _bus = bus;
            _services = cvm;

            GlobalFocusedPart = gfc;

            AttachedRecipe = vm;
            _unConfirmedName = vm.Name;

            var expandAllRecipeItemsSubscription = bus.Subscribe<SetAllTheRecipeItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Subscriptions.Add(expandAllRecipeItemsSubscription);
        }

        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        [RelayCommand]
        public async Task CommitNameChangeAsync()
        {
            if (!IsRenaming) return;

            var args = new PartSetValueInfo<RecipeViewModel, string>(AttachedRecipe, UnConfirmedName);
            await _services.RecipeService.RenameRecipe(args);

            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = AttachedRecipe.Name;
            IsRenaming = false;
        }

        [RelayCommand]
        public void StartRenaming()
                => IsRenaming = true;

        [RelayCommand]
        public void ExpandBranch()
        {
            IsExpanded = true;
        }

        [RelayCommand]
        public void CollapseBranch()
        {
            IsExpanded = false;
        }

        [RelayCommand]
        public void FindInTree()
        {
            var query = AttachedRecipe.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Recipe);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void ToggleGlobalFocus()
        {
            ToggleFocused(GlobalFocusedPart);
        }

        [RelayCommand]
        public void ToggleLocalFocus(IIsolatedFocusedPart target)
        {
            ToggleFocused(target);
        }
    }
}
