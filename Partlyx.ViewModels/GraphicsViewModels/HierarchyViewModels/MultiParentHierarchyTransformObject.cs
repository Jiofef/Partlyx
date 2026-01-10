using System.Numerics;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class MultiParentHierarchyTransformObject : ObservableMultiParentHierarchyObject, ISizePositionObject
    {
        private float _width, _height;
        public float Width { get => _width; protected set => SetProperty(ref _width, value); }
        public float Height { get => _height; protected set => SetProperty(ref _height, value); }

        public Vector2 GetSize() => new Vector2(Width, Height);

        // Primary global positions
        private float _xGlobal, _yGlobal;

        public float X
        {
            get => _xGlobal;
            set => SetProperty(ref _xGlobal, value);
        }

        public float Y
        {
            get => _yGlobal;
            set => SetProperty(ref _yGlobal, value);
        }

        public Vector2 GetPosition() => new Vector2(X, Y);

        public float XCentered { get => X + Width / 2; set => X = value - Width / 2; }
        public float YCentered { get => Y + Height / 2; set => Y = value - Height / 2; }

        public Vector2 GetPositionCentered() => new Vector2(XCentered, YCentered);

        public float XLocal => Parents.Count == 0 ? X : X - GetParentsAverageX();

        public float YLocal => Parents.Count == 0 ? Y : Y - GetParentsAverageY();

        public float XLocalCentered { get => XLocal + Width / 2; }
        public float YLocalCentered { get => YLocal + Height / 2; }



        private float GetParentsAverageX()
        {
            if (Parents.Count == 0) return 0;
            return Parents.Average(p => (p as MultiParentHierarchyTransformObject)?.X ?? 0);
        }

        private float GetParentsAverageY()
        {
            if (Parents.Count == 0) return 0;
            return Parents.Average(p => (p as MultiParentHierarchyTransformObject)?.Y ?? 0);
        }
    }
}
