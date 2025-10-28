using Avalonia;
using Avalonia.Controls;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections;
using System.Linq;

namespace Partlyx.UI.Avalonia.Behaviors;

public static class ListViewSelectionBehavior
{
    public static readonly AttachedProperty<IList> BindableSelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<Control, IList>(
            "BindableSelectedItems",
            null);

    public static void SetBindableSelectedItems(ListBox element, IList value)
        => element.SetValue(BindableSelectedItemsProperty, value);

    public static IList GetBindableSelectedItems(ListBox element)
        => element.GetValue(BindableSelectedItemsProperty);

    static ListViewSelectionBehavior()
    {
        BindableSelectedItemsProperty.Changed.AddClassHandler<ListBox>((lb, e) =>
            OnBindableSelectedItemsChanged(lb, e));
    }

    private static void OnBindableSelectedItemsChanged(ListBox lb, AvaloniaPropertyChangedEventArgs e)
    {
        lb.SelectionChanged -= Lv_SelectionChanged;

        if (e.NewValue is IList newList)
        {
            lb.SelectionMode = SelectionMode.Multiple;
            SyncListViewToTarget(lb, newList);
            lb.SelectionChanged += Lv_SelectionChanged;
        }
    }

    private static void Lv_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox lb) return;
        var target = GetBindableSelectedItems(lb);
        if (target == null) return;

        try
        {
            // Remove removed items
            foreach (var item in e.RemovedItems)
                if (target.Contains(item))
                    target.Remove(item);

            // Add new items  
            foreach (var item in e.AddedItems)
                if (!target.Contains(item))
                    target.Add(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ListViewSelectionBehavior error: " + ex);
        }
    }

    private static void SyncListViewToTarget(ListBox lv, IList target)
    {
        try
        {
            if (lv.SelectedItems == null) return;

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