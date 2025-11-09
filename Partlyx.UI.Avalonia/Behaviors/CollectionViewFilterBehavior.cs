using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class CollectionViewFilterBehavior : Behavior<ItemsControl>
    {
        public static readonly StyledProperty<Func<object, bool>?> PredicateProperty =
            AvaloniaProperty.Register<CollectionViewFilterBehavior, Func<object, bool>?>(nameof(Predicate));

        public Func<object, bool>? Predicate
        {
            get => GetValue(PredicateProperty);
            set => SetValue(PredicateProperty, value);
        }

        private IEnumerable? _originalSource;
        private INotifyCollectionChanged? _sourceNotifier;
        private readonly ObservableCollection<object> _filtered = new ObservableCollection<object>();

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject == null) return;
            AssociatedObject.PropertyChanged += AssociatedObject_PropertyChanged;
            PropertyChanged += Behavior_PropertyChanged;
            CaptureOriginalSource();
            AttachToSourceCollection(_originalSource);
            ApplyFilter();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.PropertyChanged -= AssociatedObject_PropertyChanged;
            }
            PropertyChanged -= Behavior_PropertyChanged;
            DetachFromSourceCollection();
            if (AssociatedObject != null)
            {
                AssociatedObject.ItemsSource = _originalSource as IEnumerable;
            }
            _filtered.Clear();
            _originalSource = null;
        }

        private void AssociatedObject_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
            {
                CaptureOriginalSource();
                DetachFromSourceCollection();
                AttachToSourceCollection(_originalSource);
                ApplyFilter();
            }
        }

        private void Behavior_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == PredicateProperty)
                ApplyFilter();
        }

        private void CaptureOriginalSource()
        {
            if (AssociatedObject == null) return;
            _originalSource = AssociatedObject.ItemsSource ?? AssociatedObject.Items;
        }

        private void AttachToSourceCollection(IEnumerable? source)
        {
            if (source is INotifyCollectionChanged notifier)
            {
                _sourceNotifier = notifier;
                _sourceNotifier.CollectionChanged += SourceCollectionChanged;
            }
        }

        private void DetachFromSourceCollection()
        {
            if (_sourceNotifier != null)
            {
                _sourceNotifier.CollectionChanged -= SourceCollectionChanged;
                _sourceNotifier = null;
            }
        }

        private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (AssociatedObject == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                IEnumerable sourceEnumerable = _originalSource ?? Enumerable.Empty<object>();
                var items = sourceEnumerable.Cast<object>();
                IEnumerable<object> accepted;
                if (Predicate != null)
                    accepted = items.Where(o => Predicate(o));
                else
                    accepted = items;

                _filtered.Clear();

                if (AssociatedObject.Items.Count > 0)
                    AssociatedObject.Items.Clear();

                foreach (var it in accepted)
                    _filtered.Add(it);
            });
        }
    }
}
