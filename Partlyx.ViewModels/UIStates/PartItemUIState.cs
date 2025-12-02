using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIStates
{
    public abstract partial class PartItemUIState : PartlyxObservable, IDisposable
    {
        public abstract IVMPart AttachedPart { get; }

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

        // Focusing
        private bool _hasGlobalFocus;
        public bool HasGlobalFocus { get => _hasGlobalFocus; private set => SetProperty(ref _hasGlobalFocus, value); }
        protected void SetFocused(IFocusedPart focusedContainer, bool doFocus)
        {
            if (doFocus)
            {
                focusedContainer.FocusPart(AttachedPart);
                HasGlobalFocus = true;
            }
            else if (focusedContainer.FocusedPart == AttachedPart)
            {
                focusedContainer.FocusPart(null);
            }
        }
        protected void ToggleFocused(IFocusedPart focusedContainer)
        {
             SetFocused(focusedContainer, focusedContainer.FocusedPart != AttachedPart);
        }

        public abstract void FindInTree();
        [RelayCommand]
        public void SetGlobalFocus(bool doFocus) => SetFocused(AttachedPart.GlobalNavigations.FocusedPart, doFocus);
        public void SetLocalFocus(IIsolatedFocusedPart target, bool doFocus) => SetFocused(target, doFocus);
        [RelayCommand]
        public void ToggleGlobalFocus() => ToggleFocused(AttachedPart.GlobalNavigations.FocusedPart);
        [RelayCommand]
        public void ToggleLocalFocus(IIsolatedFocusedPart target) => ToggleFocused(target);
        [RelayCommand]
        public void FocusGlobal() => SetGlobalFocus(true);
        [RelayCommand]
        public void UnfocusGlobal() => SetGlobalFocus(false);
        [RelayCommand]
        public void FocusLocal(IIsolatedFocusedPart target) => SetLocalFocus(target, true);
        [RelayCommand]
        public void UnfocusLocal(IIsolatedFocusedPart target) => SetLocalFocus(target, false);
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
