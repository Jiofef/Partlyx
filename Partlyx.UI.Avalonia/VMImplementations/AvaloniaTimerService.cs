using Avalonia.Threading;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaTimerService : ITimerService
    {
        private readonly DispatcherTimer _timer;

        public event EventHandler? Tick
        {
            add => _timer.Tick += value;
            remove => _timer.Tick -= value;
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool IsRunning => _timer.IsEnabled;

        public AvaloniaTimerService()
        {
            _timer = new DispatcherTimer();
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();
    }
}
