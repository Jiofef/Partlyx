using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentSumControllerBinder : IDisposable
    {
        public ObservableCollection<ComponentGraphNodeViewModel> ComponentsCollection { get; }
        public ComponentSumController SumController { get; }

        public ComponentSumControllerBinder(ObservableCollection<ComponentGraphNodeViewModel> componentsCollection, ComponentSumController controller)
        {
            ComponentsCollection = componentsCollection;
            SumController = controller;

            ComponentsCollection.CollectionChanged += OnLeafsCollectionChanged;
        }

        private void OnLeafsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) return;

                    foreach (ComponentGraphNodeViewModel item in e.NewItems)
                    {
                        SumController.AddComponentNode(item);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) return;

                    foreach (ComponentGraphNodeViewModel item in e.OldItems)
                    {
                        SumController.RemoveComponentNode(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    SumController.ClearComponents();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null || e.NewItems == null) return;
                    foreach (ComponentGraphNodeViewModel item in e.NewItems)
                    {
                        SumController.AddComponentNode(item);
                    }
                    foreach (ComponentGraphNodeViewModel item in e.OldItems)
                    {
                        SumController.RemoveComponentNode(item);
                    }
                    break;
            }
        }

        public void Dispose()
        {
            ComponentsCollection.CollectionChanged -= OnLeafsCollectionChanged;
        }
    }
}