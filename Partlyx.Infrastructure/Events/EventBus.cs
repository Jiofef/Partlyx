using System.Collections.Concurrent;
using System.Reflection;

namespace Partlyx.Infrastructure.Events;

public sealed partial class EventBus : IEventBus, IDisposable
{
    public EventBus() { }
    private sealed class Subscription
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Type EventType { get; init; } = null!;
        public MethodInfo Method { get; init; } = null!;
        public WeakReference? TargetRef { get; init; } // Null for static or strong
        public bool IsStatic { get; init; }
        public bool IsAsync { get; init; }
        public SynchronizationContext? SyncContext { get; init; }
        public bool IsStrong { get; init; }
        public Delegate? StrongHandler { get; init; }

        // Optimized invokers
        public Action<object?, object?>? SyncInvoker { get; set; }       // (target, event) => void
        public Func<object?, object?, Task>? AsyncInvoker { get; set; }  // (target, event) => Task
    }

    private readonly ConcurrentDictionary<Type, List<Subscription>> _subs = new();
    private readonly object _lock = new();

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler, bool captureSynchronizationContext = false)
        => AddSubscription(handler, captureSynchronizationContext, isAsync: false, strong: false);

    public IDisposable SubscribeAsync<TEvent>(Func<TEvent, Task> handler, bool captureSynchronizationContext = false)
        => AddSubscription(handler, captureSynchronizationContext, isAsync: true, strong: false);

    public IDisposable SubscribeStrong<TEvent>(Action<TEvent> handler, bool captureSynchronizationContext = false)
        => AddSubscription(handler, captureSynchronizationContext, isAsync: false, strong: true);

    public IDisposable SubscribeStrongAsync<TEvent>(Func<TEvent, Task> handler, bool captureSynchronizationContext = false)
        => AddSubscription(handler, captureSynchronizationContext, isAsync: true, strong: true);

    private IDisposable AddSubscription(Delegate handler, bool captureSync, bool isAsync, bool strong)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        var method = handler.Method;
        var target = handler.Target;
        var parameters = method.GetParameters();
        if (parameters.Length != 1) throw new ArgumentException("Handler must take exactly one parameter (the event).");

        var eventType = parameters[0].ParameterType;

        var sub = new Subscription
        {
            EventType = eventType,
            Method = method,
            TargetRef = (target == null ? null : (strong ? null : new WeakReference(target))),
            IsStatic = target == null,
            IsAsync = isAsync,
            SyncContext = captureSync ? SynchronizationContext.Current : null,
            IsStrong = strong,
            StrongHandler = strong ? handler : null
        };

        // Try to create optimized invoker
        try
        {
            if (!isAsync)
                sub.SyncInvoker = CreateSyncInvoker(method, eventType);
            else
                sub.AsyncInvoker = CreateAsyncInvoker(method, eventType);
        }
        catch
        {
            // fallback: leave invokers null -> we'll use MethodInfo.Invoke slow-path
        }

        _subs.AddOrUpdate(eventType,
            _ => { var list = new List<Subscription> { sub }; return list; },
            (_, list) => { lock (_lock) list.Add(sub); return list; });

        return new SubscriptionDisposable(this, eventType, sub.Id);
    }

    private sealed class SubscriptionDisposable : IDisposable
    {
        private readonly EventBus _bus;
        private readonly Type _eventType;
        private readonly Guid _id;
        private bool _disposed;
        public SubscriptionDisposable(EventBus bus, Type eventType, Guid id)
        {
            _bus = bus; _eventType = eventType; _id = id;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _bus.RemoveSubscription(_eventType, _id);
            _disposed = true;
        }
    }

    private void RemoveSubscription(Type type, Guid id)
    {
        if (!_subs.TryGetValue(type, out var list)) return;
        lock (_lock)
        {
            var item = list.FirstOrDefault(s => s.Id == id);
            if (item != null) list.Remove(item);
            if (list.Count == 0) _subs.TryRemove(type, out _);
        }
    }

    public void Dispose() => _subs.Clear();
}
