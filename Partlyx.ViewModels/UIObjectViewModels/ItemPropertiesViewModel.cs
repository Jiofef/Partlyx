using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ItemPropertiesViewModel : ObservableObject, IDisposable
    {
        private readonly IDisposable _focusedPartChangedSubscription;

        public IGlobalFocusedPart FocusedPart { get; }
        public ItemPropertiesViewModel(IGlobalFocusedPart focusedPart, IEventBus bus)
        {
            FocusedPart = focusedPart;

            _focusedPartChangedSubscription = bus.Subscribe<FocusedPartChangedEvent>(ev => OnFocusedPartChanged());
        }

        public void OnFocusedPartChanged()
        {

        }

        public void Dispose()
        {
            _focusedPartChangedSubscription.Dispose();
        }
    }
}
