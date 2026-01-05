using System.Numerics;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class SingleParentHierarchyTransformObject : ObservableSingleParentHierarchyObject, ISizePositionObject
    {
        private float _width, _height;
        public float Width { get => _width; protected set => SetProperty(ref _width, value); }
        public float Height { get => _height; protected set => SetProperty(ref _height, value); }

        public Vector2 GetSize() => new Vector2(Width, Height);

        public float X
        {
            get => Parent is SingleParentHierarchyTransformObject parent ? parent.X + XLocal : XLocal;
            set
            {
                XLocal = Parent is SingleParentHierarchyTransformObject parent ? parent.X + value : value;
            }
        }
        public float Y
        {
            get => Parent is SingleParentHierarchyTransformObject parent ? parent.Y + YLocal : YLocal;
            set
            {
                YLocal = Parent is SingleParentHierarchyTransformObject parent ? parent.Y + value : value;
            }
        }

        public Vector2 GetPosition() => new Vector2(X, Y);

        public float XCentered { get => X + Width / 2; set => X = value - Width / 2; }
        public float YCentered { get => Y + Height / 2; set => Y = value - Height / 2; }

        public Vector2 GetPositionCentered() => new Vector2(XCentered, YCentered);

        private float _xLocal, _yLocal;
        public float XLocal { get => _xLocal; set { _xLocal = value; NotifyPositionXChanged(); NotifyChildrenPositionXChange(); } }
        public float YLocal { get => _yLocal; set { _yLocal = value; NotifyPositionYChanged(); NotifyChildrenPositionYChange(); } }

        public float XLocalCentered { get => X + Width / 2; set => XLocal = value - Width / 2; }
        public float YLocalCentered { get => Y + Height / 2; set => YLocal = value - Height / 2; }

        // If you're going to set all the local positions and update tree at one time
        public void SetXLocalSilent(float value) => _xLocal = value;
        public void SetYLocalSilent(float value) => _yLocal = value;
        public void SetXLocalCenteredSilent(float value) => _xLocal = value - Width / 2;
        public void SetYLocalCenteredSilent(float value) => _yLocal = value - Height / 2;

        public void UpdateGlobalPositionOfTree()
        {
            // Root finding
            SingleParentHierarchyTransformObject root = this;
            while (root.Parent is SingleParentHierarchyTransformObject parent)
            {
                root = parent;
            }

            root.NotifyPositionXChanged();
            root.NotifyPositionYChanged();

            root.NotifyChildrenPositionXChange();
            root.NotifyChildrenPositionYChange();
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
            foreach (SingleParentHierarchyTransformObject child in Children)
            {
                child.NotifyPositionXChanged();
                child.NotifyChildrenPositionXChange();
            }
        }
        private void NotifyChildrenPositionYChange()
        {
            foreach (SingleParentHierarchyTransformObject child in Children)
            {
                child.NotifyPositionYChanged();
                child.NotifyChildrenPositionYChange();
            }
        }
    }
}
