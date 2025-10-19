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
        private readonly RecipeViewModel _recipeVM;

        public RecipeItemUIState(RecipeViewModel vm, PartsServiceViewModel cvm, IEventBus bus) 
        {
            _bus = bus;
            _services = cvm;

            _recipeVM = vm;
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

            var args = new PartSetValueInfo<RecipeViewModel, string>(_recipeVM, UnConfirmedName);
            await _services.RecipeService.RenameRecipe(args);

            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = _recipeVM.Name;
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
            var query = _recipeVM.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Recipe);
            _bus.Publish(ev);
        }
    }
}
