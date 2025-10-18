using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class GlobalFocusedPart : FocusedPartAbstract, IGlobalFocusedPart, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly List<IDisposable> _subscriptions = new();
        public GlobalFocusedPart(IEventBus bus) 
        {
            _bus = bus;

            var fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => FocusPart(null));
            _subscriptions.Add(fileClosedSubscription);
        }

        protected override void OnPartFocused(IVMPart? part, bool isValueChanged)
        {
            base.OnPartFocused(part, isValueChanged);

            if (!isValueChanged) return;

            if (part != null)
            {
                var @event = new PartFocusedEvent((PartTypeEnumVM)SelectedPartType!, FocusedPart!.Uid);
                _bus.Publish(@event);
            }
            else
            {
                var @event = new PartUnfocusedEvent();
                _bus.Publish(@event);
            }

            var @changedEvent = new FocusedPartChangedEvent(SelectedPartType, FocusedPart?.Uid);
            _bus.Publish(changedEvent);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}