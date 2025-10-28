using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Partlyx.UI.Avalonia.Behaviors;

public static class ListViewSelectionBehavior
{
    public static readonly DependencyProperty BindableSelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "BindableSelectedItems",
            typeof(IList),
            typeof(ListViewSelectionBehavior),
            new PropertyMetadata(null, OnBindableSelectedItemsChanged));

    public static void SetBindableSelectedItems(DependencyObject o, IList value) => o.SetValue(BindableSelectedItemsProperty, value);
    public static IList GetBindableSelectedItems(DependencyObject o) => (IList)o.GetValue(BindableSelectedItemsProperty);

    private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView lv) return;

        lv.SelectionChanged -= Lv_SelectionChanged;

        if (e.NewValue is IList newList)
        {
            lv.SelectionMode = SelectionMode.Extended;
            SyncListViewToTarget(lv, newList);
            lv.SelectionChanged += Lv_SelectionChanged;
        }
    }

    private static void Lv_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView lv) return;
        var target = GetBindableSelectedItems(lv);
        if (target == null) return;

        // Reentrance preventing
        try
        {
            // Remove removed
            foreach (var item in e.RemovedItems)
                if (target.Contains(item))
                    target.Remove(item);

            // Add new
            foreach (var item in e.AddedItems)
                if (!target.Contains(item))
                    target.Add(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ListViewSelectionBehavior error: " + ex);
        }
    }

    // When first setting the binding - synchronize ListView.SelectedItems <- target (if target already contains elements),
    // for the UI to show already selected elements.
    private static void SyncListViewToTarget(ListView lv, IList target)
    {
        try
        {
            lv.SelectedItems.Clear();
            foreach (var it in target)
                lv.SelectedItems.Add(it);
        }
        catch
        {
            // Ignore
        }
    }
}
