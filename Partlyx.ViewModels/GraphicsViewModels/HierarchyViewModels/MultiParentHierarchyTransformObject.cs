using System.Numerics;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class MultiParentHierarchyTransformObject : ObservableMultiParentHierarchyObject, ISizePositionObject
    {
        private float _width, _height;
        public float Width { get => _width; protected set => SetProperty(ref _width, value); }
        public float Height { get => _height; protected set => SetProperty(ref _height, value); }

        public Vector2 GetSize() => new Vector2(Width, Height);

        // Cached global positions to avoid expensive calculations
        private float _cachedX;
        private float _cachedY;
        private bool _isPositionCacheValid = false;

        public float X
        {
            get
            {
                if (!_isPositionCacheValid)
                    UpdatePositionCache();
                return _cachedX;
            }
            set
            {
                if (Parents.Count == 0)
                {
                    XLocal = value;
                }
                else
                {
                    float parentsAvgX = GetParentsAverageX();
                    XLocal = value - parentsAvgX;
                }
                InvalidatePositionCache();
            }
        }

        public float Y
        {
            get
            {
                if (!_isPositionCacheValid)
                    UpdatePositionCache();
                return _cachedY;
            }
            set
            {
                if (Parents.Count == 0)
                {
                    YLocal = value;
                }
                else
                {
                    float parentsAvgY = GetParentsAverageY();
                    YLocal = value - parentsAvgY;
                }
                InvalidatePositionCache();
            }
        }

        public Vector2 GetPosition() => new Vector2(X, Y);

        public float XCentered { get => X + Width / 2; set => X = value - Width / 2; }
        public float YCentered { get => Y + Height / 2; set => Y = value - Height / 2; }

        public Vector2 GetPositionCentered() => new Vector2(XCentered, YCentered);

        private float _xLocal, _yLocal;
        public float XLocal
        {
            get => _xLocal;
            set
            {
                if (_xLocal != value)
                {
                    _xLocal = value;
                    InvalidatePositionCache();
                    NotifyPositionXChanged();
                    NotifyChildrenPositionXChange();
                }
            }
        }

        public float YLocal
        {
            get => _yLocal;
            set
            {
                if (_yLocal != value)
                {
                    _yLocal = value;
                    InvalidatePositionCache();
                    NotifyPositionYChanged();
                    NotifyChildrenPositionYChange();
                }
            }
        }

        public float XLocalCentered { get => X + Width / 2; set => XLocal = value - Width / 2; }
        public float YLocalCentered { get => Y + Height / 2; set => YLocal = value - Height / 2; }

        // If you're going to set all the local positions and update tree at one time
        public void SetXLocalSilent(float value) => _xLocal = value;
        public void SetYLocalSilent(float value) => _yLocal = value;
        public void SetXLocalCenteredSilent(float value) => _xLocal = value - Width / 2;
        public void SetYLocalCenteredSilent(float value) => _yLocal = value - Height / 2;

        private void UpdatePositionCache()
        {
            if (Parents.Count == 0)
            {
                _cachedX = XLocal;
                _cachedY = YLocal;
            }
            else
            {
                _cachedX = GetParentsAverageX() + XLocal;
                _cachedY = GetParentsAverageY() + YLocal;
            }
            _isPositionCacheValid = true;
        }

        private void InvalidatePositionCache()
        {
            if (_isPositionCacheValid)
            {
                _isPositionCacheValid = false;
                // Invalidate cache for all descendants
                foreach (var child in GetAllTheDescendants())
                {
                    if (child is MultiParentHierarchyTransformObject mphto)
                    {
                        mphto._isPositionCacheValid = false;
                    }
                }
            }
        }

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
            // Invalidate all position caches in the hierarchy
            var allObjects = GetAllTheAncestors().Concat(GetAllTheDescendants()).Distinct();
            foreach (var obj in allObjects)
            {
                if (obj is MultiParentHierarchyTransformObject mphto)
                {
                    mphto._isPositionCacheValid = false;
                }
            }

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
