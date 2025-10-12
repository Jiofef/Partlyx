using CommunityToolkit.Mvvm.ComponentModel;
using System.Numerics;

namespace Partlyx.ViewModels.GraphicsViewModels
{
    public class FromToLineViewModel : ObservableObject
    {
        public FromToLineViewModel() { }
        public FromToLineViewModel(Vector2 from, Vector2 to)
        {
            _from = from;
            _to = to;
        }

        private Vector2 _from;
        public Vector2 From { get => _from; set => SetProperty(ref _from, value); }

        private Vector2 _to;
        public Vector2 To { get => _to; set => SetProperty(ref _to, value); }
    }
}
