using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.GraphicsViewModels;
using System.Drawing;
using System.Numerics;

namespace Partlyx.ViewModels.UIServices
{
    public partial class PanAndZoomControllerViewModel : ObservableObject, IPositionObject
    {
        private float _panPositionX, _panPositionY;

        /// <summary> Pan position X </summary>
        public float X { get => _panPositionX; set => SetProperty(ref _panPositionX, value); }
        /// <summary> Pan position Y </summary>
        public float Y { get => _panPositionY; set => SetProperty(ref _panPositionY, value); }

        private float _elementWidth;
        public float ElementWidth
        {
            get => _elementWidth;
            set 
            {
                SetProperty(ref _elementWidth, value); 
            }
        }

        private float _elementHeight;
        public float ElementHeight
        {
            get => _elementHeight;
            set
            {
                SetProperty(ref _elementHeight, value);
            }
        }

        private double _zoomLevel;
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, Math.Clamp(value, _minZoom, _maxZoom)); }

        private double _minZoom = 0.1;
        public double MinZoom { get => _minZoom; set => SetProperty(ref _minZoom, value); }

        private double _maxZoom = 10;
        public double MaxZoom { get => _maxZoom; set => SetProperty(ref _maxZoom, value); }

        public void CenterizePanPosition(Point where)
        {
            ZoomLevel = 1.0;
            X = -where.X + ElementWidth / 2;
            Y = -where.Y + ElementHeight / 2;
        }
    }
}
