using Avalonia.Threading;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Threading.Tasks;
namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaDispatcherInvoker : IDispatcherInvoker
    {
        private readonly Dispatcher _dispatcher = Dispatcher.UIThread;
        public bool CheckAccess() => _dispatcher.CheckAccess();
        public void Invoke(Action action) => _dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
        public async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> task) => await _dispatcher.InvokeAsync(task);

        public void BeginInvoke(Action action) => _dispatcher.Post(action);
    }
}