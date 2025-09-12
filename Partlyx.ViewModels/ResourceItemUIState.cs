using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.ViewModels.PartsViewModels;
namespace Partlyx.ViewModels
{
    public partial class ResourceItemUIState : ObservableObject
    {
        private readonly ICommandServices _commands;
        private readonly ResourceItemViewModel _resourceVM;

        public ResourceItemUIState(ResourceItemViewModel vm, ICommandServices cs)
        {
            _commands = cs;

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

            await _commands.CreateAsyncEndExcecuteAsync<SetNameToResourceCommand>(_resourceVM.Uid, UnConfirmedName);
            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = _resourceVM.Name;
            IsRenaming = false;
        }
    }
}
