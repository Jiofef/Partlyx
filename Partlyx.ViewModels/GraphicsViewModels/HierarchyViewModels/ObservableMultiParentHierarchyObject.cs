using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class ObservableMultiParentHierarchyObject : ObservableObject
    {
        public ObservableMultiParentHierarchyObject()
        {
            Children = new(_childrenList);
            Parents = new(_parentsList);
        }

        private HashSet<ObservableMultiParentHierarchyObject> _childrenSet = new();
        protected ObservableCollection<ObservableMultiParentHierarchyObject> _childrenList = new();
        public ReadOnlyObservableCollection<ObservableMultiParentHierarchyObject> Children { get; }

        private HashSet<ObservableMultiParentHierarchyObject> _parentsSet = new();
        private ObservableCollection<ObservableMultiParentHierarchyObject> _parentsList = new();
        public ReadOnlyObservableCollection<ObservableMultiParentHierarchyObject> Parents { get; }
        // Add/remove
        public bool AddParent(ObservableMultiParentHierarchyObject parent)
        {
            if (AddParentOneSide(parent))
            {
                parent.AddChildOneSide(this);
                return true;
            }

            return false;
        }
        protected bool AddParentOneSide(ObservableMultiParentHierarchyObject parent)
        {
            if (_parentsSet.Add(parent))
            {
                _parentsList.Add(parent);
                return true;
            }
            return false;
        }
        public bool RemoveParent(ObservableMultiParentHierarchyObject parent)
        {
            if (RemoveParentOneSide(parent))
            {
                parent.RemoveChildOneSide(this);
                return true;
            }

            return false;
        }
        protected bool RemoveParentOneSide(ObservableMultiParentHierarchyObject parent)
        {
            if (_parentsSet.Remove(parent))
            {
                _parentsList.Remove(parent);
                return true;
            }
            return false;
        }
        public bool AddChild(ObservableMultiParentHierarchyObject child)
        {
            if (AddChildOneSide(child))
            {
                child.AddParentOneSide(this);
                return true;
            }

            return false;
        }
        protected bool AddChildOneSide(ObservableMultiParentHierarchyObject child)
        {
            if (_childrenSet.Add(child))
            {
                _childrenList.Add(child);
                return true;
            }
            return false;
        }
        public bool RemoveChild(ObservableMultiParentHierarchyObject child)
        {
            if (RemoveChildOneSide(child))
            {
                child.RemoveParentOneSide(this);
                return true;
            }

            return false;
        }
        protected bool RemoveChildOneSide(ObservableMultiParentHierarchyObject child)
        {
            if (_childrenSet.Remove(child))
            {
                _childrenList.Remove(child);
                return true;
            }
            return false;
        }

        // Other functions

        public bool HasParent(ObservableMultiParentHierarchyObject parent)
            => _parentsSet.Contains(parent);
        public bool HasChild(ObservableMultiParentHierarchyObject parent)
            => _childrenSet.Contains(parent);
        public ObservableMultiParentHierarchyObject? GetSingleRootOrNull()
            => GetRoots().SingleOrDefault();

        public List<ObservableMultiParentHierarchyObject> GetRoots()
        {
            var ancestors = GetAllTheAncestors();

            return ancestors.Where(a => a.Parents.Count == 0).ToList();
        }

        /// <summary> Performs the action for all hierarchy elements from top to bottom, starting with this </summary>
        protected void ExcecuteWithAllTheChildren(Action<ObservableMultiParentHierarchyObject> action)
        {
            var uniqueChildren = GetAllTheDescendants();

            foreach (var child in uniqueChildren)
                action(child);
        }

        public List<ObservableMultiParentHierarchyObject> GetAllTheDescendants()
        {
            var uniqueChildren = new HashSet<ObservableMultiParentHierarchyObject>();
            var uniqueChildrenOrdered = new List<ObservableMultiParentHierarchyObject>();

            AddUniqueRecursion(this);
            void AddUniqueRecursion(ObservableMultiParentHierarchyObject hObject)
            {
                if (uniqueChildren.Contains(hObject))
                    return;

                foreach (ObservableMultiParentHierarchyObject child in hObject.Children)
                {
                    AddUniqueRecursion(child);

                    if (uniqueChildren.Add(child))
                        uniqueChildrenOrdered.Add(child);
                }
            }

            return uniqueChildrenOrdered;
        }
        public List<ObservableMultiParentHierarchyObject> GetAllTheAncestors()
        {
            var uniqueParents = new HashSet<ObservableMultiParentHierarchyObject>();
            var uniqueParentsOrdered = new List<ObservableMultiParentHierarchyObject>();

            AddUniqueRecursion(this);
            void AddUniqueRecursion(ObservableMultiParentHierarchyObject hObject)
            {
                if (uniqueParents.Contains(hObject))
                    return;

                foreach (ObservableMultiParentHierarchyObject parent in hObject.Parents)
                {
                    AddUniqueRecursion(parent);

                    if (uniqueParents.Add(parent))
                        uniqueParentsOrdered.Add(parent);
                }
            }

            return uniqueParentsOrdered;
        }
    }
}
