using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public class DynamicPositionController : IDynamicPositionController
    {
        private readonly ITimerService _timerService;
        private float _velocityX = 1f;
        private float _velocityY = 1f;
        private bool _isEnabled = true;

        public bool IsEnabled { get => _isEnabled; set { _isEnabled = value; UpdateTimerEnabled(); } }
        public float VelocityX { get => _velocityX; set { _velocityX = value; UpdateTimerEnabled(); } }
        public float VelocityY { get => _velocityY; set { _velocityY = value; UpdateTimerEnabled(); } }

        public TimeSpan Interval { get => _timerService.Interval; set => _timerService.Interval = value; }
        public IPositionObject TargetObject { get; set; }

        public DynamicPositionController(IPositionObject targetObject, ITimerService timer)
        {
            _timerService = timer;
            Interval = TimeSpan.FromSeconds(1.0 / 60.0);

            TargetObject = targetObject;

            _timerService.Tick += Update;
        }

        private bool _timerEnabled { get; set; }

        private void UpdateTimerEnabled()
        {
            bool isTimerShouldBeEnabled = IsEnabled && (VelocityX != 0 || VelocityY != 0);

            if (isTimerShouldBeEnabled == _timerEnabled)
                return;

            if (isTimerShouldBeEnabled)
                _timerService.Start();
            else
                _timerService.Stop();

            _timerEnabled = isTimerShouldBeEnabled;
        }

        protected virtual void Update(object? sender, EventArgs args)
        {
            TargetObject.X += VelocityX;
            TargetObject.Y += VelocityY;
        }
    }
}
