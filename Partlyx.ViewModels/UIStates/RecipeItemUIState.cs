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
        public RecipeItemUIState(RecipeViewModel vm, IEventBus bus, PartsServiceViewModel cvm, IGlobalFocusedPart gfc) 
        {
            _bus = bus;
            _services = cvm;

            AttachedRecipe = vm;
            _unConfirmedName = vm.Name;

            var expandAllRecipeItemsSubscription = bus.Subscribe<SetAllTheRecipeItemsExpandedEvent>(ev => SetExpanded(ev.expand));
            Disposables.Add(expandAllRecipeItemsSubscription);
        }

        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        public override async Task HandleDrop(ISelectedParts droppedParts)
        {
            var dropType = droppedParts.GetOnlyNotEmptyCollectionPartsTypeOrNull();
            if (dropType == null) return;

            if (dropType == PartTypeEnumVM.Resource)
            {
                var resources = droppedParts.Resources.ToList();
                await _services.ComponentService.CreateComponentsFromAsync(AttachedRecipe, resources);
            }
            else if (dropType == PartTypeEnumVM.Component)
            {
                var components = droppedParts.Components.ToList();
                await _services.ComponentService.MoveComponentsAsync(AttachedRecipe, components);
            }
        }

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
        public override void FindInTree()
        {
            var query = AttachedRecipe.Name;
            var ev = new TreeSearchQueryEvent(query, PartTypeEnumVM.Recipe);
            _bus.Publish(ev);
            IsSelected = true;
        }
    }
}
