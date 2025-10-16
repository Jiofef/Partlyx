using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Implementations;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Partlyx.ViewModels.ItemProperties
{
    public partial class TextBoxItemPropertyViewModel : ItemPropertyViewModel
    {
        public TextBoxItemPropertyViewModel() 
        {
            ConfirmChangesTask = SaveChanges;
        }

        private string _text = "";
        public string Text { get => _text;
            set => SetProperty(ref _text, value); }

        private bool _isChangingText;
        public bool IsChangingText { get => _isChangingText; set => SetProperty(ref _isChangingText, value); }

        public Func<object?, Task>? ConfirmChangesTask { get; set; }
        public Func<object?, Task>? CancelChangesTask { get; set; }

        [RelayCommand]
        public async Task ConfirmChanges(object? args)
        {
            if (!IsChangingText) return;

            if (ConfirmChangesTask != null)
                await ConfirmChangesTask(args);

            IsChangingText = false;
        }

        [RelayCommand]
        public async Task CancelChanges(object? args)
        {
            if (CancelChangesTask != null)
                await CancelChangesTask(args);

            IsChangingText = false;
        }

        [RelayCommand]
        public void StartTyping()
                => IsChangingText = true;
    }
}
