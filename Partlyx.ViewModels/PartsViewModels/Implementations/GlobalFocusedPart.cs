using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class GlobalFocusedPart : FocusedPartAbstract, IGlobalFocusedPart
    {
        private readonly IEventBus _bus;
        public GlobalFocusedPart(IEventBus bus) 
        {
            _bus = bus;
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
    }
}