using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.Graph;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class ObservableHierarchyObject : ObservableObject
    {
        public ObservableHierarchyObject()
        {
            Children = new(_children);
        }

        protected ObservableCollection<ObservableHierarchyObject> _children = new();
        public ReadOnlyObservableCollection<ObservableHierarchyObject> Children { get; }

        private ObservableHierarchyObject? _parent;
        public ObservableHierarchyObject? Parent { get => _parent; private set => SetProperty(ref _parent, value); }

        public void Reparent(ObservableHierarchyObject newParent)
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
            }
            newParent.AddChild(this);
        }
        public ObservableHierarchyObject GetRoot()
        {
            var root = this;

            while (root.Parent is ObservableHierarchyObject parent)
            {
                root = parent;
            }

            return root;
        }
        public void AddChild(ObservableHierarchyObject child)
        {
            if (_children.Contains(child)) return;
            _children.Add(child);
            child.Parent = this;
        }
        public void RemoveChild(ObservableHierarchyObject child)
        {
            if (_children.Remove(child))
                child.Parent = null;
        }
        public void RemoveChildByIndex(int index)
        {
            RemoveChild(_children[index]);
        }

        /// <summary> Performs the action for all hierarchy elements from top to bottom, starting with this </summary>
        protected void ExcecuteWithAllTheChildren(Action<ObservableHierarchyObject> action)
        {
            ExcecuteRecursion(this);
            void ExcecuteRecursion(ObservableHierarchyObject hObject)
            {
                foreach (ObservableHierarchyObject child in hObject.Children)
                {
                    action(child);
                    ExcecuteRecursion(child);
                }
            }
        }

        public TParent? TryFindParent<TParent>() where TParent : ObservableHierarchyObject
        {
            var current = this;

            do
            {
                current = current.Parent;
            }
            while (current is not TParent && current is not null);

            return (TParent?)current;
        }
    }
}
