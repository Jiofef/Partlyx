using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Helpers
{
    public static class UIThreadHelper
    {
        public static Task<T> RunOnUIThreadAsync<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            var dispatcher = App.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
            {
                try { tcs.SetResult(action()); } catch (Exception ex) { tcs.SetException(ex); }
            }
            else
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    try { tcs.SetResult(action()); } catch (Exception ex) { tcs.SetException(ex); }
                }));
            }

            return tcs.Task;
        }

        public static Task RunOnUIThreadAsync(Action action)
            => RunOnUIThreadAsync(() => { action(); return true; });
    }
}
