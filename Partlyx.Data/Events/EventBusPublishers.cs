namespace Partlyx.Infrastructure.Events;
public sealed partial class EventBus : IEventBus, IDisposable
{
    public void Publish<TEvent>(TEvent @event) => PublishAsync(@event).GetAwaiter().GetResult();

    public async Task PublishAsync<TEvent>(TEvent @event)
    {
        var type = typeof(TEvent);
        if (!_subs.TryGetValue(type, out var list)) return;

        Subscription[] snapshot;
        lock (_lock) snapshot = list.ToArray();

        List<Exception>? exceptions = null;
        var toRemove = new List<(Type, Guid)>();

        foreach (var sub in snapshot)
        {
            object? targetInstance = null;
            if (!sub.IsStatic)
            {
                if (sub.IsStrong)
                {
                    targetInstance = sub.StrongHandler?.Target;
                }
                else
                {
                    var wr = sub.TargetRef;
                    if (wr == null || !wr.IsAlive) { toRemove.Add((sub.EventType, sub.Id)); continue; }
                    targetInstance = wr.Target;
                    if (targetInstance == null) { toRemove.Add((sub.EventType, sub.Id)); continue; }
                }
            }

            try
            {
                if (!sub.IsAsync)
                {
                    // fast path: optimized invoker exists
                    if (sub.SyncInvoker != null)
                    {
                        if (sub.SyncContext != null)
                        {
                            sub.SyncContext.Post(_ => sub.SyncInvoker(targetInstance, @event), null);
                        }
                        else
                        {
                            sub.SyncInvoker(targetInstance, @event);
                        }
                    }
                    else
                    {
                        // fallback to reflection
                        if (sub.SyncContext != null)
                        {
                            sub.SyncContext.Post(_ => sub.Method.Invoke(targetInstance, new object[] { @event }), null);
                        }
                        else
                        {
                            sub.Method.Invoke(targetInstance, new object[] { @event });
                        }
                    }
                }
                else
                {
                    if (sub.AsyncInvoker != null)
                    {
                        if (sub.SyncContext != null)
                        {
                            var tcs = new TaskCompletionSource<object?>();
                            sub.SyncContext.Post(async _ =>
                            {
                                try { await sub.AsyncInvoker(targetInstance, @event).ConfigureAwait(false); tcs.SetResult(null); }
                                catch (Exception ex) { tcs.SetException(ex); }
                            }, null);
                            await tcs.Task.ConfigureAwait(false);
                        }
                        else
                        {
                            await sub.AsyncInvoker(targetInstance, @event).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // fallback: reflection invoke and cast to Task if possible
                        if (sub.SyncContext != null)
                        {
                            var tcs = new TaskCompletionSource<object?>();
                            sub.SyncContext.Post(async _ =>
                            {
                                try
                                {
                                    var res = sub.Method.Invoke(targetInstance, new object[] { @event });
                                    if (res is Task t) await t.ConfigureAwait(false);
                                    tcs.SetResult(null);
                                }
                                catch (Exception ex) { tcs.SetException(ex); }
                            }, null);
                            await tcs.Task.ConfigureAwait(false);
                        }
                        else
                        {
                            var res = sub.Method.Invoke(targetInstance, new object[] { @event });
                            if (res is Task t) await t.ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(ex);
            }
        }

        if (toRemove.Count > 0)
            foreach (var (t, id) in toRemove) RemoveSubscription(t, id);

        if (exceptions != null) throw new AggregateException(exceptions);
    }
}