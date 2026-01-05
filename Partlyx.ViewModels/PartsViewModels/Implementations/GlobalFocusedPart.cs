using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class GlobalFocusedPart : FocusedElementContainerAbstract, IGlobalFocusedElementContainer, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly List<IDisposable> _subscriptions = new();
        public GlobalFocusedPart(IEventBus bus) 
        {
            _bus = bus;

            var fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => Focus(null));
            _subscriptions.Add(fileClosedSubscription);
        }

        protected override void OnElementFocused(IFocusable? element, IFocusable? previousElement, bool isValueChanged)
        {
            base.OnElementFocused(element, previousElement, isValueChanged);

            if (!isValueChanged) return;


            if (element != null)
            {
                var @event = new GlobalElementFocusedEvent(element, previousElement);
                _bus.Publish(@event);
            }
            else
            {
                var @event = new GlobalElementUnfocusedEvent();
                _bus.Publish(@event);
            }

            var @changedEvent = new GlobalFocusedElementChangedEvent(element, previousElement);
            _bus.Publish(@changedEvent);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
