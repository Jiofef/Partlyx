using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.GraphicsViewModels
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
    }

}
