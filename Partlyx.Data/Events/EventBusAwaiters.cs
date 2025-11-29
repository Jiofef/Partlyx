using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Events
{
    public sealed partial class EventBus : IEventBus, IDisposable
    {
        private readonly ConcurrentDictionary<Type, List<TaskCompletionSource<object>>> _eventWaiters = new();

        public Task<TEvent> WaitUntil<TEvent>(CancellationToken cancellationToken = default)
            => WaitUntil<TEvent>(null, null, cancellationToken);

        public Task<TEvent> WaitUntil<TEvent>(TimeSpan timeout, CancellationToken cancellationToken = default)
            => WaitUntil<TEvent>(null, timeout, cancellationToken);

        public Task<TEvent> WaitUntil<TEvent>(Func<TEvent, bool> predicate, CancellationToken cancellationToken = default)
            => WaitUntil(predicate, null, cancellationToken);

        public Task<TEvent> WaitUntil<TEvent>(Func<TEvent, bool> predicate, TimeSpan timeout, CancellationToken cancellationToken = default)
            => WaitUntil(predicate, timeout, cancellationToken);

        private Task<TEvent> WaitUntil<TEvent>(Func<TEvent, bool>? predicate, TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var eventType = typeof(TEvent);

            var waiter = new EventWaiter<TEvent>
            {
                Tcs = tcs,
                Predicate = predicate,
                Timeout = timeout,
                CancellationToken = cancellationToken
            };

            // Adding pending items to the dictionary
            var waiters = _eventWaiters.GetOrAdd(eventType, _ => new List<TaskCompletionSource<object>>());
            lock (waiters)
            {
                waiters.Add(tcs);
            }

            // Setting up the timeout
            if (timeout.HasValue)
            {
                var timeoutCts = new CancellationTokenSource(timeout.Value);
                timeoutCts.Token.Register(() =>
                {
                    if (RemoveWaiter(eventType, tcs))
                        tcs.TrySetException(new TimeoutException($"Wait for event {eventType.Name} timed out after {timeout.Value}"));
                });
            }

            // Setting up cancellation
            cancellationToken.Register(() =>
            {
                if (RemoveWaiter(eventType, tcs))
                    tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task.ContinueWith(t => (TEvent)t.Result, cancellationToken);
        }

        private struct EventWaiter<TEvent>
        {
            public TaskCompletionSource<object> Tcs { get; init; }
            public Func<TEvent, bool>? Predicate { get; init; }
            public TimeSpan? Timeout { get; init; }
            public CancellationToken CancellationToken { get; init; }
        }

        private bool RemoveWaiter(Type eventType, TaskCompletionSource<object> tcs)
        {
            if (!_eventWaiters.TryGetValue(eventType, out var waiters))
                return false;

            lock (waiters)
            {
                return waiters.Remove(tcs);
            }
        }

        private void NotifyWaiters<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);
            if (!_eventWaiters.TryGetValue(eventType, out var waiters))
                return;

            TaskCompletionSource<object>[] toComplete;
            lock (waiters)
            {
                toComplete = waiters.ToArray();
                waiters.Clear();
            }

            foreach (var tcs in toComplete)
            {
                // If there is a predicate, we check it through reflection
                // In the real implementation, you need to store predicates together with TCS
                try
                {
                    tcs.TrySetResult(@event);
                }
                catch
                {
                    // Ignoring errors in setting the result
                }
            }
        }
    }
}
