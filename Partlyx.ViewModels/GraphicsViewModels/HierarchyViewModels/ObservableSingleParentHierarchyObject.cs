using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.Graph;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class ObservableSingleParentHierarchyObject : ObservableObject
    {
        public ObservableSingleParentHierarchyObject()
        {
            Children = new(_children);
        }

        protected ObservableCollection<ObservableSingleParentHierarchyObject> _children = new();
        public ReadOnlyObservableCollection<ObservableSingleParentHierarchyObject> Children { get; }

        private ObservableSingleParentHierarchyObject? _parent;
        public ObservableSingleParentHierarchyObject? Parent { get => _parent; private set => SetProperty(ref _parent, value); }

        public void Reparent(ObservableSingleParentHierarchyObject newParent)
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
            }
            newParent.AddChild(this);
        }
        public ObservableSingleParentHierarchyObject GetRoot()
        {
            var root = this;

            while (root.Parent is ObservableSingleParentHierarchyObject parent)
            {
                root = parent;
            }

            return root;
        }
        public void AddChild(ObservableSingleParentHierarchyObject child)
        {
            if (_children.Contains(child)) return;
            _children.Add(child);
            child.Parent = this;
        }
        public void RemoveChild(ObservableSingleParentHierarchyObject child)
        {
            if (_children.Remove(child))
                child.Parent = null;
        }
        public void RemoveChildByIndex(int index)
        {
            RemoveChild(_children[index]);
        }

        /// <summary> Performs the action for all hierarchy elements from top to bottom, starting with this </summary>
        protected void ExcecuteWithAllTheChildren(Action<ObservableSingleParentHierarchyObject> action)
        {
            ExcecuteRecursion(this);
            void ExcecuteRecursion(ObservableSingleParentHierarchyObject hObject)
            {
                foreach (ObservableSingleParentHierarchyObject child in hObject.Children)
                {
                    action(child);
                    ExcecuteRecursion(child);
                }
            }
        }

        public TParent? TryFindParent<TParent>() where TParent : ObservableSingleParentHierarchyObject
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
