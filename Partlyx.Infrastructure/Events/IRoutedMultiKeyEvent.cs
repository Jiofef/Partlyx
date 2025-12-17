
namespace Partlyx.Infrastructure.Events
{
    public interface IRoutedMultiKeyEvent : IKeyedEvent
    {
        HashSet<object> ReceiverKeys { get; }

        IEnumerable<object> IKeyedEvent.GetRoutingKeys() => ReceiverKeys;
    }
}