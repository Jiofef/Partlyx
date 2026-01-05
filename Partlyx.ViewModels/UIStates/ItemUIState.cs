using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIStates
{
    public abstract partial class ItemUIState : PartlyxObservable
    {
        public abstract object AttachedObject { get; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        [RelayCommand]
        public void SetSelected(bool value) => IsSelected = value;
        [RelayCommand]
        public void Select() => SetSelected(true);
        [RelayCommand]
        public void Unselect() => SetSelected(false);

        // Expanding
        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

        [RelayCommand]
        public void SetExpanded(bool isExpanded) => IsExpanded = isExpanded;

        [RelayCommand]
        public void Expand() => SetExpanded(true);
        [RelayCommand]
        public void Collapse() => SetExpanded(false);
    }
}
