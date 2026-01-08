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
            set
            {
                if (_xGlobal != value)
                {
                    _xGlobal = value;
                    NotifyPositionXChanged();
                    NotifyChildrenPositionXChange();
                }
            }
        }

        public float Y
        {
            get => _yGlobal;
            set
            {
                if (_yGlobal != value)
                {
                    _yGlobal = value;
                    NotifyPositionYChanged();
                    NotifyChildrenPositionYChange();
                }
            }
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

        public void UpdateGlobalPositionOfTree()
        {
            // Find all roots and update them
            var roots = GetRoots();
            foreach (var root in roots)
            {
                if (root is MultiParentHierarchyTransformObject mphto)
                {
                    mphto.NotifyPositionXChanged();
                    mphto.NotifyPositionYChanged();
                    mphto.NotifyChildrenPositionXChange();
                    mphto.NotifyChildrenPositionYChange();
                }
            }
        }

        private void NotifyPositionXChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(ISizePositionObject.XCentered));
            OnPropertyChanged(nameof(XLocal));
            OnPropertyChanged(nameof(XLocalCentered));
        }

        private void NotifyPositionYChanged()
        {
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(ISizePositionObject.YCentered));
            OnPropertyChanged(nameof(YLocal));
            OnPropertyChanged(nameof(YLocalCentered));
        }

        private void NotifyChildrenPositionXChange()
        {
            foreach (MultiParentHierarchyTransformObject child in Children)
            {
                child.NotifyPositionXChanged();
                child.NotifyChildrenPositionXChange();
            }
        }

        private void NotifyChildrenPositionYChange()
        {
            foreach (MultiParentHierarchyTransformObject child in Children)
            {
                child.NotifyPositionYChanged();
                child.NotifyChildrenPositionYChange();
            }
        }
    }
}
