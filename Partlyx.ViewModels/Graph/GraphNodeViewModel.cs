using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Partlyx.ViewModels.Graph
{
    public class GraphNodeViewModel : ObservableObject, IElementWithSize
    {
        public const float StandardNodeWidth = 96;
        public const float StandardNodeHeight = 64;

        private float _width = StandardNodeWidth, _height = StandardNodeHeight;
        public float Width { get => _width; protected set => SetProperty(ref _width, value); }
        public float Height { get => _height; protected set => SetProperty(ref _height, value); }

        public GraphNodeViewModel(Guid valueUid, ReadOnlyObservableCollection<Guid>? childrenUids = null, object? value = null)
        {
            ValueUid = valueUid;
            ChildrenUids = childrenUids;
            _value = value;
        }

        public Guid ValueUid { get; }
        public Guid Uid { get; } = Guid.NewGuid();

        private float _x, _y;
        public float X { get => _x; set { SetProperty(ref _x, value); OnPropertyChanged(nameof(XCentered)); } }
        public float Y { get => _y; set { SetProperty(ref _y, value); OnPropertyChanged(nameof(YCentered)); } }

        public float XCentered { get => _x + Width / 2; set => X = value - Width / 2; }
        public float YCentered { get => _y + Height / 2; set => Y = value - Height / 2; }

        public Vector2 GetSize() => new Vector2(Width, Height);
        // By default, the size of the child node is assumed to be equal to the parent, but this can be overridden
        public virtual Vector2 GetChildSize(int childIndex)
        {
            if (ChildrenUids == null || childIndex >= ChildrenUids.Count) 
                throw new ArgumentException();

            return new Vector2(Width, Height);
        }

        public ReadOnlyObservableCollection<Guid>? ChildrenUids { get; init; }

        private object? _value;
        public object? Value { get => _value; set => SetProperty(ref _value, value); }
    }
}
