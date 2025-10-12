using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public class ObservablePoint : ObservableObject, IPositionObject
    {
        public ObservablePoint() { }
        public ObservablePoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        private float _x;
        private float _y;

        public float X { get => _x; set => SetProperty(ref _x, value); }
        public float Y { get => _y; set => SetProperty(ref _y, value); }
    }
}
