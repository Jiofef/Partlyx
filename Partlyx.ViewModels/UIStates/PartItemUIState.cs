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
    public abstract partial class PartItemUIState : ObservableObject, IDisposable
    {
        public abstract IVMPart AttachedPart { get; }
        protected readonly List<IDisposable> Subscriptions = new();

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        private bool _isExpanded = false;

        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

        private bool _hasGlobalFocus;
        public bool HasGlobalFocus { get => _hasGlobalFocus; private set => SetProperty(ref _hasGlobalFocus, value); }

        public void SetExpanded(bool isExpanded) => IsExpanded = isExpanded;

        public void Dispose()
        {
            foreach(var subscription in Subscriptions)
                subscription.Dispose();
        }

        protected void ToggleFocused(IFocusedPart focusedContainer)
        {
            var previousFocused = focusedContainer.FocusedPart;

            if (focusedContainer.FocusedPart == AttachedPart)
                focusedContainer.FocusPart(null);
            else
            {
                focusedContainer.FocusPart(AttachedPart);
                HasGlobalFocus = true;
            }

            if (previousFocused != null)
                previousFocused.UiItem.NotifyUnfocused();
        }

        private void NotifyUnfocused()
        {
            HasGlobalFocus = false;
        }

        public virtual Task HandleDrop(ISelectedParts droppedParts) { return Task.CompletedTask; }
        
    }
}
