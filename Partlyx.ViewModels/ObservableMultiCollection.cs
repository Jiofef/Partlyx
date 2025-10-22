using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace UJL.CSharp.Collections
{
    /// <summary> 
    /// Observable collection containing all items of the child observable collections. Added items appear at the end of the list.
    /// When you add new collections, items appear at the end of the list in the order they are in the added collection.
    /// </summary>
    public class ObservableMultiCollection<T> : ReadOnlyObservableCollection<T>
    {
        public Action<INotifyCollectionChanged> CollectionAdded = delegate { };
        public Action<INotifyCollectionChanged> CollectionRemoved = delegate { };

        private List<INotifyCollectionChanged> _collections { get; } = new();
        public IReadOnlyList<INotifyCollectionChanged> Collections => _collections;

        private ObservableCollection<T> _multiCollection;

        public ObservableMultiCollection(params ObservableCollection<T>[] collections) : base(new ObservableCollection<T>())
        {
            _multiCollection = (ObservableCollection<T>)Items;
            foreach (var collection in collections)
                AddCollection(collection);

            _multiCollection.CollectionChanged += (sender, args) =>
                CollectionChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) return;
                    foreach (T item in e.NewItems!)
                        _multiCollection.Add(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) return;
                    foreach (T item in e.OldItems)
                        _multiCollection.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null || e.NewItems == null) return;
                    var item1 = (T)e.OldItems[0]!;
                    _multiCollection.Remove(item1);
                    var item2 = (T)e.NewItems[0]!;
                    _multiCollection.Add(item2);
                    break;
                default:
                    UpdateItems();
                    break;
            }
        }

        public void AddCollection<TObservableCollection>(TObservableCollection collection) where TObservableCollection : ICollection<T>, INotifyCollectionChanged
        {
            _collections.Add(collection);
            SubscribeCollection(collection);
            CollectionAdded?.Invoke(collection);
        }
        public void AddCollections<TObservableCollection>(ICollection<TObservableCollection> collections) where TObservableCollection : ICollection<T>, INotifyCollectionChanged
        {
            foreach (var collection in collections)
                AddCollection(collection);
        }
        public void RemoveCollectionAt(int index)
        {
            var collection = _collections[index];
            _collections.RemoveAt(index);
            UnsubscribeCollection(collection);

            foreach (var item in (ICollection<T>)collection)
                _multiCollection.Remove(item);

            CollectionRemoved?.Invoke(collection);
        }
        public void RemoveCollection<TObservableCollection>(TObservableCollection collection) where TObservableCollection : ICollection<T>, INotifyCollectionChanged
        {
            _collections.Remove(collection);
            UnsubscribeCollection(collection);

            foreach (var item in collection)
                _multiCollection.Remove(item);

            CollectionRemoved?.Invoke(collection);
        }
        public void RemoveCollections<TObservableCollection>(ICollection<TObservableCollection> collections) where TObservableCollection : ICollection<T>, INotifyCollectionChanged
        {
            foreach (var collection in collections)
                RemoveCollection(collection);
        }

        private void SubscribeCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnCollectionChanged;
        }
        private void UnsubscribeCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnCollectionChanged;
        }

        public void ClearCollections()
        {
            _collections.Clear();
            _multiCollection.Clear();
        }

        private void UpdateItems()
        {
            _multiCollection.Clear();

            foreach (var collection in _collections)
            {
                foreach (var item in (ICollection<T>)collection)
                {
                    _multiCollection.Add(item);
                }
            }
        }

        new public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    }
}
