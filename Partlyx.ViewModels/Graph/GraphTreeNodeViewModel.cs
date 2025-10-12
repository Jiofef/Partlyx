using Partlyx.ViewModels.GraphicsViewModels;
using System.Collections.ObjectModel;
using System.Numerics;
using UJL.CSharp.Collections;

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

        /// <summary> Although the node itself does not contain logic for creating connected lines, it contains a collection for ease of storage when created externally</summary>
        public ObservableCollection<FromToLineViewModel> ConnectedLines { get; } = new();

        public ObservableMultiCollection<FromToLineViewModel> GetBranchLinesMultiCollection()
        {
            var collections = GetBranchLinesCollections().ToArray();

            var multiCollection = new ObservableMultiCollection<FromToLineViewModel>(collections);
            return multiCollection;
        }
        public ObservableCollection<FromToLineViewModel>[] GetBranchLinesCollections()
        {
            List<ObservableCollection<FromToLineViewModel>> collections = new();
            AddBranchLinesCollectionsToList(collections);

            return collections.ToArray();
        }
        private void AddBranchLinesCollectionsToList(List<ObservableCollection<FromToLineViewModel>> collections)
        {
            collections.Add(ConnectedLines);

            foreach (GraphTreeNodeViewModel child in Children)
                child.AddBranchLinesCollectionsToList(collections);
        }

        public void UpdateChildrenPositions()
        {
            BuildBranchAndGetItsWidth();
            UpdateGlobalPositionOfTree();
        }

        // This method works almost without errors, but requires rework and removal of workarounds (for example adding Width to branchOffset)
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

            // Setting children positions
            float branchOffset = -branchWidth / 2 + Width / 4 + Width / 8;

            float nextChildOffsetX = branchOffset;
            for (int i = 0; i < Children.Count; i++)
            {
                float currentBranchWidth = childrenBranchWidths[i];
                var child = (GraphTreeNodeViewModel)Children[i];
                child.SetYLocalSilent(Height + SingleChildrenDistanceY);
                // Adding currentBranchWidth / 2 to move the parent node to its branch center
                child.SetXLocalSilent(nextChildOffsetX + currentBranchWidth / 2);

                nextChildOffsetX += childrenBranchWidths[i] + SingleChildrenDistanceX;
            }

            if (Children.Count == 1)
                ((GraphTreeNodeViewModel)Children[0]).SetXLocalSilent(0);

            return branchWidth;
        }
    }
}
