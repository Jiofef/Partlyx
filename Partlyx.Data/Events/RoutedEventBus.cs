using System.Collections.Concurrent;
using System.Reflection;

namespace Partlyx.Infrastructure.Events
{
    public class RoutedEventBus : IRoutedEventBus
    {
        // Storage: EventType -> SubscriptionManagerForThatType
        private readonly ConcurrentDictionary<Type, IRoutedTypeManager> _typeManagers = new();
        private readonly IEventBus _underlyingBus;

        public RoutedEventBus(IEventBus underlyingBus)
        {
            _underlyingBus = underlyingBus;
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : IKeyedEvent => _underlyingBus.Publish(@event);
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IKeyedEvent => _underlyingBus.PublishAsync(@event);

        // --- Subscription Method Implementation (Proxying) ---

        // Action Subscriptions
        public IDisposable Subscribe<TEvent>(object key, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent
            => AddActionSubscriptionInternal<TEvent>(new[] { key }, handler, isStrong: false, captureCtx);
        public IDisposable Subscribe<TEvent>(IEnumerable<object> keys, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent
             => AddActionSubscriptionInternal<TEvent>(keys, handler, isStrong: false, captureCtx);
        public IDisposable SubscribeStrong<TEvent>(object key, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent
            => AddActionSubscriptionInternal<TEvent>(new[] { key }, handler, isStrong: true, captureCtx);
        public IDisposable SubscribeStrong<TEvent>(IEnumerable<object> keys, Action<TEvent> handler, bool captureCtx = false) where TEvent : IKeyedEvent
             => AddActionSubscriptionInternal<TEvent>(keys, handler, isStrong: true, captureCtx);

        // Func Subscriptions
        public IDisposable SubscribeAsync<TEvent>(object key, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent
            => AddFuncSubscriptionInternal<TEvent>(new[] { key }, handler, isStrong: false, captureCtx);
        public IDisposable SubscribeAsync<TEvent>(IEnumerable<object> keys, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent
             => AddFuncSubscriptionInternal<TEvent>(keys, handler, isStrong: false, captureCtx);
        public IDisposable SubscribeStrongAsync<TEvent>(object key, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent
            => AddFuncSubscriptionInternal<TEvent>(new[] { key }, handler, isStrong: true, captureCtx);
        public IDisposable SubscribeStrongAsync<TEvent>(IEnumerable<object> keys, Func<TEvent, Task> handler, bool captureCtx = false) where TEvent : IKeyedEvent
             => AddFuncSubscriptionInternal<TEvent>(keys, handler, isStrong: true, captureCtx);


        // --- Internal Subscription Creation Logic ---

        private IDisposable AddActionSubscriptionInternal<TEvent>(IEnumerable<object> keys, Action<TEvent> handler, bool isStrong, bool captureCtx)
            where TEvent : IKeyedEvent
        {
            var manager = _typeManagers.GetOrAdd(typeof(TEvent), _ => new RoutedTypeManager<TEvent>(_underlyingBus));

            if (manager is RoutedTypeManager<TEvent> typedManager)
            {
                // 1. Create wrapper based on original Action<TEvent>
                IEventHandlerWrapper<TEvent> wrapper = isStrong
                    ? new StrongActionHandlerWrapper<TEvent>(handler)
                    : new WeakActionHandlerWrapper<TEvent>(handler);

                // 2. Apply context decorator
                if (captureCtx && SynchronizationContext.Current != null)
                {
                    wrapper = new SynchronizationContextWrapper<TEvent>(wrapper, SynchronizationContext.Current);
                }

                // 3. Add to manager's map
                return typedManager.AddSubscription(keys, wrapper);
            }
            throw new InvalidOperationException($"Type manager mismatch for {typeof(TEvent)}");
        }

        private IDisposable AddFuncSubscriptionInternal<TEvent>(IEnumerable<object> keys, Func<TEvent, Task> handler, bool isStrong, bool captureCtx)
            where TEvent : IKeyedEvent
        {
            var manager = _typeManagers.GetOrAdd(typeof(TEvent), _ => new RoutedTypeManager<TEvent>(_underlyingBus));

            if (manager is RoutedTypeManager<TEvent> typedManager)
            {
                // 1. Create wrapper based on original Func<TEvent, Task>
                IEventHandlerWrapper<TEvent> wrapper = isStrong
                    ? new StrongFuncHandlerWrapper<TEvent>(handler)
                    : new WeakFuncHandlerWrapper<TEvent>(handler);

                // 2. Apply context decorator
                if (captureCtx && SynchronizationContext.Current != null)
                {
                    wrapper = new SynchronizationContextWrapper<TEvent>(wrapper, SynchronizationContext.Current);
                }

                // 3. Add to manager's map
                return typedManager.AddSubscription(keys, wrapper);
            }
            throw new InvalidOperationException($"Type manager mismatch for {typeof(TEvent)}");
        }

        // --- Type Manager ---

        private interface IRoutedTypeManager { }

        // Class that manages subscriptions for a SPECIFIC TEvent type
        private class RoutedTypeManager<TEvent> : IRoutedTypeManager where TEvent : IKeyedEvent
        {
            private readonly ConcurrentDictionary<object, List<IEventHandlerWrapper<TEvent>>> _handlersMap = new();
            // Store a reference to the global subscription so the GC doesn't kill it
            private readonly IDisposable _globalSubscription;

            public RoutedTypeManager(IEventBus bus)
            {
                // Subscribe to the underlying bus only once for this event type.
                _globalSubscription = bus.SubscribeAsync<TEvent>(HandleEventAsync);
            }

            private async Task HandleEventAsync(TEvent @event)
            {
                var routingKeys = @event.GetRoutingKeys();
                if (routingKeys == null) return;

                // Use a Set to avoid calling the same handler multiple times 
                // if it's subscribed to several matching keys.
                var uniqueHandlersToInvoke = new HashSet<IEventHandlerWrapper<TEvent>>();

                foreach (var key in routingKeys)
                {
                    if (key != null && _handlersMap.TryGetValue(key, out var list))
                    {
                        lock (list)
                        {
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                var wrapper = list[i];
                                if (!wrapper.IsAlive)
                                {
                                    list.RemoveAt(i); // Lazy cleanup of dead weak references
                                }
                                else
                                {
                                    uniqueHandlersToInvoke.Add(wrapper);
                                }
                            }
                        }
                    }
                }

                foreach (var wrapper in uniqueHandlersToInvoke)
                {
                    await wrapper.InvokeAsync(@event);
                }
            }

            public IDisposable AddSubscription(IEnumerable<object> keys, IEventHandlerWrapper<TEvent> wrapper)
            {
                var keysList = keys.ToList();

                // Add the handler wrapper to all buckets corresponding to the keys
                foreach (var key in keysList)
                {
                    var list = _handlersMap.GetOrAdd(key, _ => new List<IEventHandlerWrapper<TEvent>>());
                    lock (list)
                    {
                        list.Add(wrapper);
                    }
                }

                // Return Disposer that removes the handler from all keys
                return new SubscriptionDisposer(() =>
                {
                    foreach (var key in keysList)
                    {
                        if (_handlersMap.TryGetValue(key, out var list))
                        {
                            lock (list)
                            {
                                list.Remove(wrapper);
                            }
                        }
                    }
                });
            }
        }

        // --- Handler Wrappers (Invocation Strategies) ---

        private interface IEventHandlerWrapper<T>
        {
            bool IsAlive { get; }
            Task InvokeAsync(T @event);
        }

        // 1. Strong Action Handler Wrapper
        private class StrongActionHandlerWrapper<T> : IEventHandlerWrapper<T>
        {
            private readonly Action<T> _handler;
            public StrongActionHandlerWrapper(Action<T> handler) => _handler = handler;

            public bool IsAlive => true; // Always alive
            public Task InvokeAsync(T @event) { _handler(@event); return Task.CompletedTask; }
        }

        // 2. Weak Action Handler Wrapper (Fix: Extracts Target from original Action to avoid closure bug)
        private class WeakActionHandlerWrapper<T> : IEventHandlerWrapper<T>
        {
            private readonly WeakReference? _targetRef;
            private readonly MethodInfo _method;
            // For static methods, Target will be null, so we store a flag
            private readonly bool _isStatic;

            public WeakActionHandlerWrapper(Action<T> handler)
            {
                // Working with Target and MethodInfo of the original Action
                _isStatic = handler.Target == null;
                if (!_isStatic)
                {
                    _targetRef = new WeakReference(handler.Target);
                }
                _method = handler.Method;
            }

            public bool IsAlive => _isStatic || (_targetRef?.IsAlive ?? false);

            public Task InvokeAsync(T @event)
            {
                if (_isStatic)
                {
                    _method.Invoke(null, new object[] { @event });
                }
                else
                {
                    var target = _targetRef!.Target;
                    if (target != null)
                    {
                        _method.Invoke(target, new object[] { @event });
                    }
                }
                return Task.CompletedTask;
            }
        }

        // 3. Strong Func Handler Wrapper
        private class StrongFuncHandlerWrapper<T> : IEventHandlerWrapper<T>
        {
            private readonly Func<T, Task> _handler;
            public StrongFuncHandlerWrapper(Func<T, Task> handler) => _handler = handler;

            public bool IsAlive => true; // Always alive
            public Task InvokeAsync(T @event) => _handler(@event);
        }

        // 4. Weak Func Handler Wrapper (Fix: Extracts Target from original Func to avoid closure bug)
        private class WeakFuncHandlerWrapper<T> : IEventHandlerWrapper<T>
        {
            private readonly WeakReference? _targetRef;
            private readonly MethodInfo _method;
            private readonly bool _isStatic;

            public WeakFuncHandlerWrapper(Func<T, Task> handler)
            {
                // Working with Target and MethodInfo of the original Func
                _isStatic = handler.Target == null;
                if (!_isStatic)
                {
                    _targetRef = new WeakReference(handler.Target);
                }
                _method = handler.Method;
            }

            public bool IsAlive => _isStatic || (_targetRef?.IsAlive ?? false);

            public Task InvokeAsync(T @event)
            {
                if (_isStatic)
                {
                    return (Task)_method.Invoke(null, new object[] { @event })!;
                }

                var target = _targetRef!.Target;
                if (target != null)
                {
                    // Invoking the Task-method via reflection
                    return (Task)_method.Invoke(target, new object[] { @event })!;
                }

                return Task.CompletedTask; // Target died
            }
        }

        // 5. Decorator for SynchronizationContext
        private class SynchronizationContextWrapper<T> : IEventHandlerWrapper<T>
        {
            private readonly IEventHandlerWrapper<T> _inner;
            private readonly SynchronizationContext _context;

            public SynchronizationContextWrapper(IEventHandlerWrapper<T> inner, SynchronizationContext context)
            {
                _inner = inner;
                _context = context;
            }

            public bool IsAlive => _inner.IsAlive;

            public Task InvokeAsync(T @event)
            {
                // If we are already on the required context, just execute
                if (SynchronizationContext.Current == _context)
                {
                    return _inner.InvokeAsync(@event);
                }

                // Otherwise, marshal the call to the required thread and AWAIT completion (TaskCompletionSource)
                var tcs = new TaskCompletionSource<bool>();

                _context.Post(async _ =>
                {
                    try
                    {
                        if (_inner.IsAlive)
                        {
                            await _inner.InvokeAsync(@event);
                        }
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }, null);

                return tcs.Task;
            }
        }

        private class SubscriptionDisposer : IDisposable
        {
            private readonly Action _action;
            public SubscriptionDisposer(Action action) => _action = action;
            public void Dispose() => _action();
        }
    }
}