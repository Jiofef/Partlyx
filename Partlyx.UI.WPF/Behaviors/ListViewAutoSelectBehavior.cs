using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Partlyx.UI.WPF.Behaviors
{
    public class ListViewAutoSelectBehavior : Behavior<ListView>
    {
        #region DependencyProperties

        public static readonly DependencyProperty WatchProperty =
            DependencyProperty.Register(nameof(Watch), typeof(object), typeof(ListViewAutoSelectBehavior),
                new PropertyMetadata(null, OnWatchChanged));

        public object? Watch
        {
            get => GetValue(WatchProperty);
            set => SetValue(WatchProperty, value);
        }

        // value to be selected
        public static readonly DependencyProperty ValueToSelectProperty =
            DependencyProperty.Register(nameof(ValueToSelect), typeof(object), typeof(ListViewAutoSelectBehavior),
                new PropertyMetadata(null, OnValueToSelectChanged));

        public object? ValueToSelect
        {
            get => GetValue(ValueToSelectProperty);
            set => SetValue(ValueToSelectProperty, value);
        }

        // if specified - compare the.ItemsSource. [CompareMemberPath] == ValueToSelect
        public static readonly DependencyProperty CompareMemberPathProperty =
            DependencyProperty.Register(nameof(CompareMemberPath), typeof(string), typeof(ListViewAutoSelectBehavior),
                new PropertyMetadata(null));

        public string? CompareMemberPath
        {
            get => (string?)GetValue(CompareMemberPathProperty);
            set => SetValue(CompareMemberPathProperty, value);
        }

        public static readonly DependencyProperty AutoScrollIntoViewProperty =
            DependencyProperty.Register(nameof(AutoScrollIntoView), typeof(bool), typeof(ListViewAutoSelectBehavior),
                new PropertyMetadata(true));

        public bool AutoScrollIntoView
        {
            get => (bool)GetValue(AutoScrollIntoViewProperty);
            set => SetValue(AutoScrollIntoViewProperty, value);
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            // if ItemsSource changes - we want to review the choice
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView))
                ?.AddValueChanged(AssociatedObject, (_, __) => ApplySelection());
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView))
                ?.RemoveValueChanged(AssociatedObject, (_, __) => ApplySelection());
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) => ApplySelection();

        private static void OnWatchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = (ListViewAutoSelectBehavior)d;
            b.ApplySelection();
        }

        private static void OnValueToSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = (ListViewAutoSelectBehavior)d;
            b.ApplySelection();
        }

        private void ApplySelection()
        {
            if (AssociatedObject == null) return;

            // Cutting out UI-thread calls
            if (!AssociatedObject.Dispatcher.CheckAccess())
            {
                AssociatedObject.Dispatcher.BeginInvoke((Action)ApplySelection, DispatcherPriority.Normal);
                return;
            }

            var itemsSource = AssociatedObject.ItemsSource ?? AssociatedObject.Items;
            if (itemsSource == null)
            {
                AssociatedObject.SelectedItem = null;
                return;
            }

            // If ValueToSelect == null - just unselect
            if (ValueToSelect == null)
            {
                AssociatedObject.SelectedItem = null;
                return;
            }

            object? found = null;

            // Shuffle items and look for a match
            foreach (var item in itemsSource)
            {
                if (item == null) continue;

                if (!string.IsNullOrEmpty(CompareMemberPath))
                {
                    // try to get the property by name (can be improved for nested paths)
                    var pi = item.GetType().GetProperty(CompareMemberPath!, BindingFlags.Public | BindingFlags.Instance);
                    if (pi != null)
                    {
                        var val = pi.GetValue(item);
                        if (AreEqual(val, ValueToSelect))
                        {
                            found = item;
                            break;
                        }
                    }
                }
                else
                {
                    // direct comparison of elements (Equals) or if ValueToSelect - key and item has matching property "Id
                    if (AreEqual(item, ValueToSelect))
                    {
                        found = item;
                        break;
                    }

                    // if ValueToSelect - scalar (Guid/int/string) and item has property "Id" or "Guid
                    var idPi = item.GetType().GetProperty("Id") ?? item.GetType().GetProperty("Guid");
                    if (idPi != null)
                    {
                        var idVal = idPi.GetValue(item);
                        if (AreEqual(idVal, ValueToSelect))
                        {
                            found = item;
                            break;
                        }
                    }
                }
            }

            if (found != null)
            {
                AssociatedObject.SelectedItem = found;
                if (AutoScrollIntoView)
                    AssociatedObject.ScrollIntoView(found);
            }
            else
            {
                // not found - unchoose (or leave the same - depending on requirements)
                AssociatedObject.SelectedItem = null;
            }
        }

        private static bool AreEqual(object? a, object? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            // if types match, use Equals
            if (a.GetType() == b.GetType())
                return a.Equals(b);

            // if one of them is a string - compare ToString()
            if (a is string || b is string)
                return string.Equals(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);

            // try to compare numeric/Guid quotes
            try
            {
                if (a is Guid || b is Guid)
                {
                    if (Guid.TryParse(a.ToString(), out var ga) && Guid.TryParse(b.ToString(), out var gb))
                        return ga == gb;
                }

                // numeric: try to convert
                var aStr = a.ToString();
                var bStr = b.ToString();
                return string.Equals(aStr, bStr, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}
