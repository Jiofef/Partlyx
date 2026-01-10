using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Partlyx.ViewModels.Graph
{
    public abstract class Path<T> : ObservableObject
    {
        protected LinkedList<T> _nodes;

        protected Path(LinkedList<T> nodes)
        {
            _nodes = nodes;
        }

        public LinkedList<T> Nodes => _nodes;

        public int Length => _nodes.Count;

        public LinkedListNode<T>? GetFirst() => _nodes.First;

        public LinkedListNode<T>? GetLast() => _nodes.Last;

        public LinkedListNode<T>? GetNext(LinkedListNode<T> node)
        {
            return node.Next;
        }

        public LinkedListNode<T>? GetPrevious(LinkedListNode<T> node)
        {
            return node.Previous;
        }

        public bool Contains(T item) => _nodes.Contains(item);

        public override string ToString()
        {
            return string.Join(" -> ", _nodes);
        }
    }
}
