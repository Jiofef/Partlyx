using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public class DynamicPanPositionController : DynamicPositionController
    {
        public PanAndZoomControllerViewModel PanController { get; set; }
        public DynamicPanPositionController(PanAndZoomControllerViewModel targetObject, ITimerService timer) : base(targetObject, timer)
        {
            PanController = targetObject;
        }

        protected override void Update(object? sender, EventArgs args)
        {
            float zoomLevel = (float)PanController.ZoomLevel;
            TargetObject.X -= VelocityX / zoomLevel;
            TargetObject.Y -= VelocityY / zoomLevel;
        }
    }
}
