using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public abstract partial class FocusedPartAbstract : ObservableObject, IFocusedPart
    {
        private IVMPart? _focusedPart;
        public IVMPart? FocusedPart { get => _focusedPart; private set => SetProperty(ref _focusedPart, value); }

        private PartTypeEnumVM? _selectedPartType;
        public PartTypeEnumVM? SelectedPartType { get => _selectedPartType; private set => SetProperty(ref _selectedPartType, value); }

        private bool _hasFocusedPart;
        public bool HasFocusedPart { get => _hasFocusedPart; private set => SetProperty(ref _hasFocusedPart, value); }

        public void FocusPart(IVMPart? part)
        {
            bool isValueChanged = part != _focusedPart;

            FocusedPart = part;
            SelectedPartType = part?.PartType;

            HasFocusedPart = part != null;

            OnPartFocused(part, isValueChanged);
        }

        protected virtual void OnPartFocused(IVMPart? part, bool isValueChanged) { }
    }
}