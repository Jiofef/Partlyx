using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;

namespace Partlyx.ViewModels.UIServices
{
    public partial class PanAndZoomControllerViewModel : ObservableObject
    {
        private double _panPositionX, _panPositionY;

        public double PanPositionX { get => _panPositionX; set => SetProperty(ref _panPositionX, value); }
        public double PanPositionY { get => _panPositionY; set => SetProperty(ref _panPositionY, value); }

        private double _elementWidth;
        public double ElementWidth
        {
            get => _elementWidth;
            set 
            {
                SetProperty(ref _elementWidth, value); 
            }
        }

        private double _elementHeight;
        public double ElementHeight
        {
            get => _elementHeight;
            set
            {
                SetProperty(ref _elementHeight, value);
            }
        }

        private double _zoomLevel;
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }

        public void CenterizePanPosition(Point where)
        {
            ZoomLevel = 1.0;
            PanPositionX = -where.X + ElementWidth / 2;
            PanPositionY = -where.Y + ElementHeight / 2;
        }
    }
}
