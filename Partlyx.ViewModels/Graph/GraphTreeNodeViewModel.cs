using Partlyx.ViewModels.GraphicsViewModels;
using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using System.Collections.ObjectModel;
using System.Numerics;
using UJL.CSharp.Collections;

namespace Partlyx.ViewModels.Graph
{
    public class GraphTreeNodeViewModel : MultiParentHierarchyTransformObject, ISizePositionObject
    {
        private const float StandardNodeDistanceX = 24;
        private const float StandardNodeDistanceY = 48;
        private const float StandardBranchDistanceX = StandardNodeDistanceX * 2.5f;

        public const float StandardNodeWidth = 96;
        public const float StandardNodeHeight = 96;

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

        /// <summary> Call it when your tree is ready </summary>
        public virtual void BuildChildren()
        {
            BuildingRecursion(this);
            void BuildingRecursion(GraphTreeNodeViewModel node)
            {
                foreach (GraphTreeNodeViewModel child in node.Children)
                {
                    child.Build();
                    BuildingRecursion(child);
                }
            }
        }

        protected virtual void Build() { }
    }
}
