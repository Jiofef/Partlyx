using CommunityToolkit.Mvvm.ComponentModel;
namespace Partlyx.ViewModels
{
    public class ResourceItemUIState : ObservableObject
    {
        private bool isSelected;
        private bool isRenaming;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isRenaming, value); }
        public bool IsRenaming { get => isRenaming; set => SetProperty(ref isRenaming, value); }
    }
}
