using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIStates
{
    public partial class PartItemUIState : ObservableObject, IDisposable
    {
        protected readonly List<IDisposable> Subscriptions = new();

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        private bool _isExpanded = false;
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

        public void SetExpanded(bool isExpanded) => IsExpanded = isExpanded;

        public void Dispose()
        {
            foreach(var subscription in Subscriptions)
                subscription.Dispose();
        }
    }
}
