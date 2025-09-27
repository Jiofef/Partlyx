using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IDispatcherInvoker
    {
        bool CheckAccess();
        void Invoke(Action action); // sync ui
        void BeginInvoke(Action action); // async ui
    }

}
