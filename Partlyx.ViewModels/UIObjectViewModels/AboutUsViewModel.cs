using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Help;
using Partlyx.ViewModels.Info;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class AboutUsWindowViewModel
    {
        private readonly IDialogService _dialogService;
        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;
        public AboutUsWindowViewModel(IDialogService ds) 
        {
            _dialogService = ds;
        }

        [RelayCommand]
        public void CloseDialog(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }
    }
}
