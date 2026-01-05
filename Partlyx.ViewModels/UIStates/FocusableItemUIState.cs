using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.UIStates
{
    public abstract partial class FocusableItemUIState : ItemUIState
    {
        public abstract IFocusedElementContainer GlobalFocusedContainer { get; }

        public override object AttachedObject => AttachedFocusable;
        public abstract IFocusable AttachedFocusable { get; }

        private bool _hasGlobalFocus;
        public bool HasGlobalFocus { get => _hasGlobalFocus; private set => SetProperty(ref _hasGlobalFocus, value); }
        protected void SetFocused(IFocusedElementContainer focusedContainer, bool doFocus)
        {
            if (doFocus)
            {
                focusedContainer.Focus(AttachedFocusable);
                HasGlobalFocus = true;
            }
            else if (focusedContainer.Focused == AttachedFocusable)
            {
                focusedContainer.Focus(null);
            }
        }
        protected void ToggleFocused(IFocusedElementContainer focusedContainer)
        {
            SetFocused(focusedContainer, focusedContainer.Focused != AttachedFocusable);
        }

        [RelayCommand]
        public void SetGlobalFocus(bool doFocus) => SetFocused(GlobalFocusedContainer, doFocus);
        public void SetLocalFocus(IIsolatedFocusedElementContainer target, bool doFocus) => SetFocused(target, doFocus);
        [RelayCommand]
        public void ToggleGlobalFocus() => ToggleFocused(GlobalFocusedContainer);
        [RelayCommand]
        public void ToggleLocalFocus(IIsolatedFocusedElementContainer target) => ToggleFocused(target);
        [RelayCommand]
        public void FocusGlobal() => SetGlobalFocus(true);
        [RelayCommand]
        public void UnfocusGlobal() => SetGlobalFocus(false);
        [RelayCommand]
        public void FocusLocal(IIsolatedFocusedElementContainer target) => SetLocalFocus(target, true);
        [RelayCommand]
        public void UnfocusLocal(IIsolatedFocusedElementContainer target) => SetLocalFocus(target, false);
        public void NotifyUnfocused()
        {
            HasGlobalFocus = false;
        }
        public void NotifyFocused()
        {
            HasGlobalFocus = true;
        }

        // Drag and drop
        public virtual Task HandleDrop(ISelectedParts droppedParts) { return Task.CompletedTask; }
    }
}
