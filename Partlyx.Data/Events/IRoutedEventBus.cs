
namespace Partlyx.Infrastructure.Events
{
    public interface IRoutedEventBus
    {
        void Publish<TEvent>(TEvent @event) where TEvent : IKeyedEvent;
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IKeyedEvent;
        IDisposable Subscribe<TEvent>(IEnumerable<object> keys, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable Subscribe<TEvent>(object key, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeAsync<TEvent>(IEnumerable<object> keys, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeAsync<TEvent>(object key, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeStrong<TEvent>(IEnumerable<object> keys, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeStrong<TEvent>(object key, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeStrongAsync<TEvent>(IEnumerable<object> keys, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
        IDisposable SubscribeStrongAsync<TEvent>(object key, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent;
    }
}