using System;
using System.Threading;
using System.Threading.Tasks;

namespace Partlyx.Services.ServiceImplementations
{
    public sealed class ThrottledInvoker
    {
        private readonly TimeSpan _delay;
        private readonly object _sync = new();

        private DateTime _lastExecution = DateTime.MinValue;
        private Task? _pending;

        public ThrottledInvoker(TimeSpan delay)
        {
            _delay = delay;
        }

        public Task InvokeAsync(Func<Task> action)
        {
            lock (_sync)
            {
                if (_pending != null && !_pending.IsCompleted)
                    return _pending;

                _pending = RunAsync(action);
                return _pending;
            }
        }

        public Task InvokeAsync(Action action)
            => InvokeAsync(() =>
            {
                action();
                return Task.CompletedTask;
            });

        public void Invoke(Action action)
            => InvokeAsync(() =>
            {
                action();
                return Task.CompletedTask;
            });

        private async Task RunAsync(Func<Task> action)
        {
            TimeSpan wait;

            lock (_sync)
            {
                var now = DateTime.UtcNow;
                var elapsed = now - _lastExecution;

                wait = elapsed < _delay
                    ? _delay - elapsed
                    : TimeSpan.Zero;
            }

            if (wait > TimeSpan.Zero)
                await Task.Delay(wait).ConfigureAwait(false);

            await action().ConfigureAwait(false);

            lock (_sync)
            {
                _lastExecution = DateTime.UtcNow;
            }
        }
    }
}