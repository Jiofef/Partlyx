using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Helpers
{
    public static class UIThreadHelper
    {
        public static Task<T> RunOnUIThreadAsync<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            Dispatcher.UIThread.Invoke(new Action(() =>
            {
                try { tcs.SetResult(action()); } catch (Exception ex) { tcs.SetException(ex); }
            }));

            return tcs.Task;
        }

        public static Task RunOnUIThreadAsync(Action action)
            => RunOnUIThreadAsync(() => { action(); return true; });
    }
}
