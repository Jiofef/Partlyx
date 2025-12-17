
namespace Partlyx.Infrastructure.Events
{
    public interface IKeyedEvent
    {
        IEnumerable<object> GetRoutingKeys();
    }
}