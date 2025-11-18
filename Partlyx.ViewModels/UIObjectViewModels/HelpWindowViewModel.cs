using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Help;
using Partlyx.ViewModels.Info;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class HelpWindowViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;
        public InfoSectionsGroupViewModel Info { get; }

        private object? _selectedItem = null;

        public object? SelectedItem { get => _selectedItem; set => SetSelectedItem(value); }
        private string? _textToShow;
        public string? TextToShow { get => _textToShow; set => SetProperty(ref _textToShow, value); }
        private void SetSelectedItem(object? value)
        {
            _selectedItem = value;
            if (value is InfoSectionViewModel info)
                TextToShow = info.ContentKey;
            else
                TextToShow = string.Empty;
        }
        public HelpWindowViewModel(IDialogService ds)
        {
            _dialogService = ds;

            Info = new InfoSectionsGroupViewModel(InfoScheme.ApplicationHelp.InfoGroup);
        }

        [RelayCommand]
        public void CloseDialog(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }
    }
}
