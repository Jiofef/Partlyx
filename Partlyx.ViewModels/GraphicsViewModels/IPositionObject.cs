using System.ComponentModel;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public interface IPositionObject : INotifyPropertyChanged
    {
        float X { get; set; }
        float Y { get; set; }
    }
}
