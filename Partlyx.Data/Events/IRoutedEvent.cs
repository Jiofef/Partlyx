
namespace Partlyx.Infrastructure.Events
{
    public interface IRoutedEvent : IKeyedEvent
    {
        object? ReceiverKey { get; }
        IEnumerable<object> IKeyedEvent.GetRoutingKeys()
        {
            if (ReceiverKey != null) yield return ReceiverKey;
        }
    }
}