using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels
{
    public partial class ResourceItemUIState : ObservableObject
    {
        private readonly PartsServiceViewModel _services;
        private readonly ResourceViewModel _resourceVM;

        public ResourceItemUIState(ResourceViewModel vm, PartsServiceViewModel svm)
        {
            _services = svm;

            _resourceVM = vm;
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
    }
}
