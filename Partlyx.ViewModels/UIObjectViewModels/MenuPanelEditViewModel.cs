using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelEditViewModel
    {
        private readonly ICommandServices _commandServices;

        public MenuPanelEditViewModel(ICommandServices cs)
        {
            _commandServices = cs;
        }

        [RelayCommand]
        public async Task Undo()
        {
            await _commandServices.Dispatcher.UndoAsync();
        }

        [RelayCommand]
        public async Task Redo()
        {
            await _commandServices.Dispatcher.RedoAsync();
        }
    }
}
