using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System;
using System.Reflection;

namespace Partlyx.UI.Avalonia.Behaviors
{
    /// <summary>
    /// Automatically selects an item in ListBox/ListView when Watch or ValueToSelect changes.
    /// Supports CompareMemberPath and auto-scroll into view.
    /// </summary>
    public class ListViewAutoSelectBehavior : Behavior<ListBox>
    {
        public static readonly StyledProperty<object?> WatchProperty =
            AvaloniaProperty.Register<ListViewAutoSelectBehavior, object?>(nameof(Watch));

        public object? Watch
        {
            get => GetValue(WatchProperty);
            set => SetValue(WatchProperty, value);
        }

        public static readonly StyledProperty<object?> ValueToSelectProperty =
            AvaloniaProperty.Register<ListViewAutoSelectBehavior, object?>(nameof(ValueToSelect));

        public object? ValueToSelect
        {
            get => GetValue(ValueToSelectProperty);
            set => SetValue(ValueToSelectProperty, value);
        }

        public static readonly StyledProperty<string?> CompareMemberPathProperty =
            AvaloniaProperty.Register<ListViewAutoSelectBehavior, string?>(nameof(CompareMemberPath));

        public string? CompareMemberPath
        {
            get => GetValue(CompareMemberPathProperty);
            set => SetValue(CompareMemberPathProperty, value);
        }

        public static readonly StyledProperty<bool> AutoScrollIntoViewProperty =
            AvaloniaProperty.Register<ListViewAutoSelectBehavior, bool>(nameof(AutoScrollIntoView), true);

        public bool AutoScrollIntoView
        {
            get => GetValue(AutoScrollIntoViewProperty);
            set => SetValue(AutoScrollIntoViewProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AttachedToVisualTree += OnLoaded;
            AssociatedObject.PropertyChanged += OnItemsSourceChanged;

            this.GetPropertyChangedObservable(WatchProperty)
                .Subscribe(_ => ApplySelection());
            this.GetPropertyChangedObservable(ValueToSelectProperty)
                .Subscribe(_ => ApplySelection());
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.AttachedToVisualTree -= OnLoaded;
                AssociatedObject.PropertyChanged -= OnItemsSourceChanged;
            }
        }

        private void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e) => ApplySelection();

        private void OnItemsSourceChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
                ApplySelection();
        }

        private void ApplySelection()
        {
            var list = AssociatedObject;
            if (list == null)
                return;

            var items = list.ItemsSource ?? list.Items;
            if (items == null)
            {
                list.SelectedItem = null;
                return;
            }

            if (ValueToSelect == null)
            {
                list.SelectedItem = null;
                return;
            }

            // если вызвано не из UI-потока
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(ApplySelection);
                return;
            }

            object? found = null;

            foreach (var item in items)
            {
                if (item == null) continue;

                if (!string.IsNullOrEmpty(CompareMemberPath))
                {
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
                    if (AreEqual(item, ValueToSelect))
                    {
                        found = item;
                        break;
                    }

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
                list.SelectedItem = found;
                if (AutoScrollIntoView)
                    list.ScrollIntoView(found);
            }
            else
            {
                list.SelectedItem = null;
            }
        }

        private static bool AreEqual(object? a, object? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            if (a.GetType() == b.GetType())
                return a.Equals(b);

            if (a is string || b is string)
                return string.Equals(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);

            try
            {
                if (a is Guid || b is Guid)
                {
                    if (Guid.TryParse(a.ToString(), out var ga) && Guid.TryParse(b.ToString(), out var gb))
                        return ga == gb;
                }

                return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}
