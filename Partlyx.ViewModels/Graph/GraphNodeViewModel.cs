using Partlyx.ViewModels.GraphicsViewModels;
using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;

namespace Partlyx.ViewModels.Graph
{
    public class GraphNodeViewModel : MultiParentHierarchyTransformObject, ISizePositionObject
    {
        public const float StandardNodeDistanceX = 24;
        public const float StandardNodeDistanceY = 48;
        public const float StandardBranchDistanceX = StandardNodeDistanceX * 2.5f;

        public const float StandardNodeWidth = 96;
        public const float StandardNodeHeight = 96;

        public GraphNodeViewModel(GraphNodeViewModel? mainRelative, object? value = null)
        {
            MainRelative = mainRelative;
            _value = value;

            Width = StandardNodeWidth;
            Height = StandardNodeHeight;

            SingleChildrenDistanceX = StandardNodeDistanceX;
            SingleChildrenDistanceY = StandardNodeDistanceY;

            BranchesDistanceX = StandardBranchDistanceX;
        }
        public Guid Uid { get; } = Guid.NewGuid();

        // By going from any node using their MainRelative, you can reach the root node.
        public GraphNodeViewModel? MainRelative { get; }
        private object? _value;
        public object? Value { get => _value; set => SetProperty(ref _value, value); }

        private float _childrenDistanceX, _childrenDistanceY;
        public float SingleChildrenDistanceX { get => _childrenDistanceX; protected set => SetProperty(ref _childrenDistanceX, value); }
        public float SingleChildrenDistanceY { get => _childrenDistanceY; protected set => SetProperty(ref _childrenDistanceY, value); }

        private float _branchesDistanceX;
        public float BranchesDistanceX { get => _branchesDistanceX; protected set => SetProperty(ref _branchesDistanceX, value); }
    }
}

