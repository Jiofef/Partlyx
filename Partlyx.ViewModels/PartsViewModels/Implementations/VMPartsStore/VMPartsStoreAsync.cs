using DynamicData;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class VMPartsStore
    {
        // Pending awaiters keyed by Uid. Protected by _pendingLock.
        private readonly object _pendingLock = new();
        private readonly Dictionary<Guid, List<AwaiterEntry>> _pendingAwaiters = new();

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Await appearance of an item with the specified Uid. If item already exists - completes immediately.
        /// If item does not exist yet - returns a task that completes when Register(...) is called for that Uid,
        /// or when timeout elapses (defaults to 5 seconds) which yields null.
        /// </summary>
        public Task<IVMPart?> TryGetAsync(Guid? itemUid, TimeSpan? timeout = null)
        {
            if (itemUid == null)
                return Task.FromResult<IVMPart?>(null);

            Guid uid = itemUid.Value;

            // Fast path: check existing dictionaries without locking.
            if (_resources.TryGetValue(uid, out var res))
                return Task.FromResult<IVMPart?>(res);
            if (_recipes.TryGetValue(uid, out var recipe))
                return Task.FromResult<IVMPart?>(recipe);
            if (_components.TryGetValue(uid, out var comp))
                return Task.FromResult<IVMPart?>(comp);

            var tcs = new TaskCompletionSource<IVMPart?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var effectiveTimeout = timeout ?? DefaultTimeout;

            // If timeout is zero or negative, complete immediately with null.
            if (effectiveTimeout <= TimeSpan.Zero)
            {
                tcs.TrySetResult(null);
                return tcs.Task;
            }

            var cts = new CancellationTokenSource(effectiveTimeout);
            var entry = new AwaiterEntry(tcs, cts);

            // Setup timeout callback: if timeout hits, remove this entry and complete with null.
            // We use Register to be able to cancel the registration when we complete via Register(...).
            CancellationTokenRegistration reg = default;
            reg = cts.Token.Register(() =>
            {
                // On timeout: attempt to remove this awaiter and complete with null.
                bool removed = false;
                lock (_pendingLock)
                {
                    if (_pendingAwaiters.TryGetValue(uid, out var list))
                    {
                        removed = list.Remove(entry);
                        if (list.Count == 0)
                            _pendingAwaiters.Remove(uid);
                    }
                }

                if (removed)
                {
                    try { entry.Tcs.TrySetResult(null); } catch { }
                }

                // dispose registration (safe to call multiple times)
                reg.Dispose();
                entry.Cts.Dispose();
            }, useSynchronizationContext: false);

            // store registration so we can dispose it later if needed
            entry.Registration = reg;

            lock (_pendingLock)
            {
                if (_pendingAwaiters.TryGetValue(uid, out var list))
                {
                    list.Add(entry);
                }
                else
                {
                    _pendingAwaiters[uid] = new List<AwaiterEntry> { entry };
                }
            }

            return tcs.Task;
        }

        /// <summary>
        /// Completes and removes all awaiters waiting for the specified uid.
        /// Called from Register(...) methods when an item is added.
        /// </summary>
        private void CompletePendingAwaiters(Guid uid, IVMPart part)
        {
            List<AwaiterEntry>? toComplete = null;

            lock (_pendingLock)
            {
                if (_pendingAwaiters.TryGetValue(uid, out var list))
                {
                    toComplete = new List<AwaiterEntry>(list);
                    _pendingAwaiters.Remove(uid);
                }
            }

            if (toComplete == null) return;

            foreach (var entry in toComplete)
            {
                try
                {
                    // Cancel the CTS so its timeout callback won't try to remove/complete again.
                    try { entry.Cts.Cancel(); } catch { }
                    // Dispose registration and CTS
                    try { entry.Registration.Dispose(); } catch { }
                    try { entry.Cts.Dispose(); } catch { }
                }
                catch { /* ignore */ }

                try { entry.Tcs.TrySetResult(part); } catch { /* ignore */ }
            }
        }

        /// <summary>
        /// Completes all pending awaiters with null (used when clearing the store).
        /// </summary>
        private void CompleteAllPendingWithNull()
        {
            Dictionary<Guid, List<AwaiterEntry>> snapshot;

            lock (_pendingLock)
            {
                if (_pendingAwaiters.Count == 0) return;
                snapshot = new Dictionary<Guid, List<AwaiterEntry>>(_pendingAwaiters);
                _pendingAwaiters.Clear();
            }

            foreach (var kv in snapshot)
            {
                foreach (var entry in kv.Value)
                {
                    try { entry.Cts.Cancel(); } catch { }
                    try { entry.Registration.Dispose(); } catch { }
                    try { entry.Cts.Dispose(); } catch { }
                    try { entry.Tcs.TrySetResult(null); } catch { }
                }
            }
        }
        /// <summary>
        /// Internal awaiter entry holding TCS + CTS and the registration for timeout.
        /// </summary>
        private class AwaiterEntry
        {
            public TaskCompletionSource<IVMPart?> Tcs { get; }
            public CancellationTokenSource Cts { get; }
            public CancellationTokenRegistration Registration { get; set; }

            public AwaiterEntry(TaskCompletionSource<IVMPart?> tcs, CancellationTokenSource cts)
            {
                Tcs = tcs;
                Cts = cts;
            }
        }   
    }
}
