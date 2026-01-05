using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Partlyx.UI.Avalonia.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public class SumHierarchyObject : ObservableObject, IDisposable
    {
        private readonly IDisposable _childrenSumChanged;

        public ObservableCollection<SumHierarchyObject> SumObjectChildren { get; } = new();

        public SumHierarchyObject() 
        {
            SumObjectChildren.CollectionChanged += (obj, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Move)
                    UpdateSum();
            };

            _childrenSumChanged = SumObjectChildren
                .ToObservableChangeSet()
                .MergeMany(item => item.WhenAnyValue(x => x.Sum))
                .Subscribe(_ => UpdateSum());
        }

        private double _baseValue = 0;
        public virtual double BaseValue { get => _baseValue; protected set => SetBaseValue(value); }

        protected void SetBaseValue(double value)
        {
            // Subtract the old value and add a new one
            Sum = Sum - _baseValue + value;

            _baseValue = value;
            OnPropertyChanged(nameof(BaseValue));
        }

        private double _sum;
        public double Sum { get => _sum; private set => SetProperty(ref _sum, value); }

        private void UpdateSum()
        {
            Sum = BaseValue + SumObjectChildren.Sum(c => c.Sum);
        }

        public void Dispose()
        {
            _childrenSumChanged.Dispose();
            SumObjectChildren.ClearAndDispose();
        }
    }
}
