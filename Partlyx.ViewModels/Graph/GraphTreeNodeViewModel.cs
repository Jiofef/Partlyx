using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Partlyx.ViewModels.Graph
{
    public class GraphTreeNodeViewModel : HierarchyTransformObject, ISizePositionObject
    {
        private const float StandardNodeDistanceX = 24;
        private const float StandardNodeDistanceY = 48;
        private const float StandardBranchDistanceX = StandardNodeDistanceX * 2;

        public const float StandardNodeWidth = 96;
        public const float StandardNodeHeight = 64;

        public GraphTreeNodeViewModel(Guid valueUid, ReadOnlyObservableCollection<Guid>? childrenUids = null, object? value = null)
        {
            ValueUid = valueUid;
            ChildrenUids = childrenUids;
            _value = value;

            Width = StandardNodeWidth;
            Height = StandardNodeHeight;

            SingleChildrenDistanceX = StandardNodeDistanceX;
            SingleChildrenDistanceY = StandardNodeDistanceY;

            BranchesDistanceX = StandardBranchDistanceX;
        }

        public Guid ValueUid { get; }
        public Guid Uid { get; } = Guid.NewGuid();

        public ReadOnlyObservableCollection<Guid>? ChildrenUids { get; init; }

        private object? _value;
        public object? Value { get => _value; set => SetProperty(ref _value, value); }

        private float _childrenDistanceX, _childrenDistanceY;
        public float SingleChildrenDistanceX { get => _childrenDistanceX; protected set => SetProperty(ref _childrenDistanceX, value); }
        public float SingleChildrenDistanceY { get => _childrenDistanceY; protected set => SetProperty(ref _childrenDistanceY, value); }

        private float _branchesDistanceX;
        public float BranchesDistanceX { get => _branchesDistanceX; protected set => SetProperty(ref _branchesDistanceX, value); }

        public void UpdateChildrenPositions()
        {
            BuildBranchAndGetItsWidth();
            UpdateGlobalPositionOfTree();
        }

        private float BuildBranchAndGetItsWidth()
        {
            if (Children.Count == 0) 
                return this.Width;

            // Finding branch width
            float branchWidth = 0;
            float[] childrenBranchWidths = new float[Children.Count];

            for (int i = 0; i < Children.Count; i++)
            {
                var child = (GraphTreeNodeViewModel)Children[i];
                float childBranchWidth = child.BuildBranchAndGetItsWidth();
                childrenBranchWidths[i] = childBranchWidth;
                branchWidth += childBranchWidth;

                
                branchWidth += child.Children.Count == 0 ? BranchesDistanceX : SingleChildrenDistanceX;
            }

            if (Children.Count >= 2)
            {
                // Additional position correct
                branchWidth -= branchWidth / Children.Count;
            }

            // Setting children positions
            float nextChildOffsetX = -branchWidth / 2;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = (GraphTreeNodeViewModel)Children[i];
                child.SetYLocalSilent(Height + SingleChildrenDistanceY);
                child.SetXLocalSilent(nextChildOffsetX);

                nextChildOffsetX += childrenBranchWidths[i] + SingleChildrenDistanceX;
            }

            return branchWidth;
        }
    }
}
