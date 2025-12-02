
namespace Partlyx.ViewModels.GraphicsViewModels
{
    public interface IDynamicPositionController
    {
        TimeSpan Interval { get; set; }
        bool IsEnabled { get; set; }
        IPositionObject TargetObject { get; set; }
        float VelocityX { get; set; }
        float VelocityY { get; set; }
    }
}