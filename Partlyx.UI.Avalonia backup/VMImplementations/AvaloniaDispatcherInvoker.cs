using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class WPFDispatcherInvoker : IDispatcherInvoker
    {
        private readonly Dispatcher _dispatcher;
        public WPFDispatcherInvoker(Dispatcher dispatcher) => _dispatcher = dispatcher;

        public bool CheckAccess() => _dispatcher.CheckAccess();
        public void Invoke(Action action) => _dispatcher.Invoke(action);
        public void BeginInvoke(Action action) => _dispatcher.BeginInvoke(action);
    }

}
