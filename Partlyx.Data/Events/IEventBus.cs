
namespace Partlyx.Infrastructure.Events
{
    public interface IEventBus
    {
        IDisposable Subscribe<TEvent>(Action<TEvent> handler, bool captureSynchronizationContext = false);
        IDisposable SubscribeAsync<TEvent>(Func<TEvent, Task> handler, bool captureSynchronizationContext = false);
        IDisposable SubscribeStrong<TEvent>(Action<TEvent> handler, bool captureSynchronizationContext = false);
        IDisposable SubscribeStrongAsync<TEvent>(Func<TEvent, Task> handler, bool captureSynchronizationContext = false);

        void Publish<TEvent>(TEvent @event);
        Task PublishAsync<TEvent>(TEvent @event);
        Task<TEvent> WaitUntil<TEvent>(CancellationToken cancellationToken = default);
        Task<TEvent> WaitUntil<TEvent>(TimeSpan timeout, CancellationToken cancellationToken = default);
        Task<TEvent> WaitUntil<TEvent>(Func<TEvent, bool> predicate, CancellationToken cancellationToken = default);
        Task<TEvent> WaitUntil<TEvent>(Func<TEvent, bool> predicate, TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}