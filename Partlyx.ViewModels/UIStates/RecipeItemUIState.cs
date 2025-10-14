using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels
{
    public partial class RecipeItemUIState : ObservableObject
    {
        private readonly PartsServiceViewModel _services;
        private readonly RecipeItemViewModel _recipeVM;

        public RecipeItemUIState(RecipeItemViewModel vm, PartsServiceViewModel cvm) 
        {
            _services = cvm;

            _recipeVM = vm;
            _unConfirmedName = vm.Name;
        }

        private bool _isSelected;
        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        [RelayCommand]
        public async Task CommitNameChangeAsync()
        {
            if (!IsRenaming) return;

            var args = new PartSetValueInfo<RecipeItemViewModel, string>(_recipeVM, UnConfirmedName);
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
    }
}
