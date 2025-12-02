using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface ITimerService
    {
        event EventHandler Tick;

        TimeSpan Interval { get; set; }

        void Start();
        void Stop();
        bool IsRunning { get; }
    }
}
