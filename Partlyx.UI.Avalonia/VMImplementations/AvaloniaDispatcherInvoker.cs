using Avalonia.Threading;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaDispatcherInvoker : IDispatcherInvoker
    {
        private readonly Dispatcher _dispatcher = Dispatcher.UIThread;
        public bool CheckAccess() => _dispatcher.CheckAccess();
        public void Invoke(Action action) => _dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
        public void BeginInvoke(Action action) => _dispatcher.Post(action);
    }
}