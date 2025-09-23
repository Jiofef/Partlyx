using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Partlyx.UI.WPF.Behaviors
{
    public class CollectionViewFilterBehavior : Behavior<ItemsControl>
    {
        public static readonly DependencyProperty PredicateProperty =
            DependencyProperty.Register(
                nameof(Predicate),
                typeof(Predicate<object>),
                typeof(CollectionViewFilterBehavior),
                new PropertyMetadata(null, OnPredicateChanged));

        public Predicate<object>? Predicate
        {
            get => (Predicate<object>?)GetValue(PredicateProperty);
            set => SetValue(PredicateProperty, value);
        }

        private static void OnPredicateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = (CollectionViewFilterBehavior)d;
            b.ApplyFilter();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            DependencyPropertyDescriptor
                .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                .AddValueChanged(AssociatedObject!, OnItemsSourceChanged);

            ApplyFilter();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                DependencyPropertyDescriptor
                    .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                    .RemoveValueChanged(AssociatedObject, OnItemsSourceChanged);

                var view = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);
                if (view != null) view.Filter = null;
            }
        }

        private void OnItemsSourceChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (AssociatedObject == null) return;
            var itemsSource = AssociatedObject.ItemsSource ?? AssociatedObject.Items;
            var view = CollectionViewSource.GetDefaultView(itemsSource);
            if (view == null) return;

            if (Predicate != null)
                view.Filter = o => Predicate!(o);
            else
                view.Filter = null;

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => view.Refresh());
            }
            else
            {
                view.Refresh();
            }
        }
    }
}
