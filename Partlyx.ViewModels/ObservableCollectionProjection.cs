using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace UJL.CSharp.Collections
{
    public class ObservableCollectionProjection<T, TBase> : ReadOnlyObservableCollection<T>
    {
        private ObservableCollection<TBase> _baseCollection;
        public ObservableCollection<TBase> BaseCollection { get => _baseCollection; set => SetBaseCollection(value); }

        private ObservableCollection<T> _projection = new();

        private Func<TBase, T> _transformer;
        public Func<TBase, T> Transformer { get => _transformer; set => SetTransformer(value); }

        public ObservableCollectionProjection(ObservableCollection<TBase> baseCollection, Func<TBase, T> transformer) : base(new ObservableCollection<T>())
        {
            _baseCollection = baseCollection;
            _projection = (ObservableCollection<T>)Items;
            _transformer = transformer;

            _baseCollection.CollectionChanged += OnCollectionChanged;
            UpdateItems();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) return;
                    int index = e.NewStartingIndex;
                    foreach (TBase item in e.NewItems!)
                    {
                        _projection.Insert(index++, Transformer(item));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) return;
                    for (int i = 0; i < e.OldItems!.Count; i ++)
                    {
                        _projection.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 0 || e.NewStartingIndex < 0) { UpdateItems(); return; }

                    _projection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 0 || e.NewStartingIndex < 0) { UpdateItems(); return; }

                    _projection.RemoveAt(e.OldStartingIndex);
                    var item2 = (TBase)e.NewItems![0]!;
                    _projection.Insert(e.NewStartingIndex, Transformer(item2));
                    break;
                default:
                    UpdateItems();
                    break;
            }
        }

        private void SetBaseCollection(ObservableCollection<TBase> collection)
        {
            if (_baseCollection != null)
                _baseCollection.CollectionChanged -= OnCollectionChanged;

            _baseCollection = collection;

            _baseCollection.CollectionChanged += OnCollectionChanged;

            UpdateItems();
            var args = new PropertyChangedEventArgs(nameof(BaseCollection));
            base.OnPropertyChanged(args);
        }


        private void SetTransformer(Func<TBase, T> transformer)
        {
            _transformer = transformer;
            UpdateItems();
            var args = new PropertyChangedEventArgs(nameof(Transformer));
            base.OnPropertyChanged(args);
        }

        private void UpdateItems()
        {
            _projection.Clear();

            foreach (var item in _baseCollection)
            {
                _projection.Add(Transformer(item));
            }
        }
    }
}
