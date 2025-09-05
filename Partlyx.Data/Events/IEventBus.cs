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
    }
}