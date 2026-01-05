using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public abstract partial class FocusedElementContainerAbstract : ObservableObject, IFocusedElementContainer
    {
        private IFocusable? _focusedPart;
        public IFocusable? Focused { get => _focusedPart; private set => SetProperty(ref _focusedPart, value); }

        private FocusableElementTypeEnum? _focusedElementType;
        public FocusableElementTypeEnum? FocusedElementType { get => _focusedElementType; private set => SetProperty(ref _focusedElementType, value); }

        private bool _hasFocusedElement;
        public bool HasFocusedElement { get => _hasFocusedElement; private set => SetProperty(ref _hasFocusedElement, value); }

        public void Focus(IFocusable? focusable)
        {
            bool isValueChanged = focusable != _focusedPart;

            var previous = Focused;

            Focused = focusable;
            FocusedElementType = focusable?.FocusableType;

            HasFocusedElement = focusable != null;

            previous?.UiItem.NotifyUnfocused();
            focusable?.UiItem.NotifyFocused();

            OnElementFocused(focusable, previous, isValueChanged);
        }

        protected virtual void OnElementFocused(IFocusable? element, IFocusable? previousElement, bool isValueChanged) { }
    }
}