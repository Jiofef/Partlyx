using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.UI.Avalonia.Behaviors
{
    /// <summary>
    /// Syncs selection between a ListBox (ListView) and an ISelectedParts implementation.
    /// Works for single and multiple selection modes.
    /// </summary>
    public class ListViewSelectedPartsSyncBehavior : Behavior<ListBox>
    {
        #region SelectedParts StyledProperty

        public static readonly StyledProperty<ISelectedParts?> SelectedPartsProperty =
            AvaloniaProperty.Register<ListViewSelectedPartsSyncBehavior, ISelectedParts?>(nameof(SelectedParts));

        public ISelectedParts? SelectedParts
        {
            get => GetValue(SelectedPartsProperty);
            set => SetValue(SelectedPartsProperty, value);
        }

        #endregion

        private bool _isUpdatingFromSelectedParts;
        private bool _isUpdatingFromListView;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
                return;

            // Attach to visual tree to ensure ScrollIntoView/Items are ready
            AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;

            // watch SelectedParts property changes
            this.GetPropertyChangedObservable(SelectedPartsProperty)
                .Subscribe(args =>
                {
                    var oldVal = (ISelectedParts?)args.OldValue;
                    var newVal = (ISelectedParts?)args.NewValue;
                    UnhookSelectedPartsCollections(oldVal);
                    HookSelectedPartsCollections(newVal);
                    // sync initial state from SelectedParts to ListBox
                    SyncAllSelectedPartsToListView();
                });

            // Subscribe list-side selection changes
            HookListSelection();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
                UnhookListSelection();
            }

            UnhookSelectedPartsCollections(SelectedParts);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // sync when the control appears
            SyncAllSelectedPartsToListView();
        }

        #region ListBox selection handling

        // Try to hook SelectedItems changes and SelectedItem changes
        private void HookListSelection()
        {
            if (AssociatedObject == null) return;

            // SelectedItems may implement INotifyCollectionChanged
            if (AssociatedObject.SelectedItems is INotifyCollectionChanged sic)
                sic.CollectionChanged += SelectedItems_CollectionChanged;

            // Also listen SelectedItem change (single selection)
            AssociatedObject.PropertyChanged += AssociatedObject_PropertyChanged;
        }

        private void UnhookListSelection()
        {
            if (AssociatedObject == null) return;

            if (AssociatedObject.SelectedItems is INotifyCollectionChanged sic)
                sic.CollectionChanged -= SelectedItems_CollectionChanged;

            AssociatedObject.PropertyChanged -= AssociatedObject_PropertyChanged;
        }

        private void AssociatedObject_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ListBox.SelectedItemProperty)
            {
                // Single selection changed
                SyncFromListViewToSelectedParts();
            }
        }

        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Multi-selection changed
            SyncFromListViewToSelectedParts(e);
        }

        #endregion

        #region SelectedParts collections hook/unhook

        private void HookSelectedPartsCollections(ISelectedParts? parts)
        {
            if (parts == null) return;

            parts.Resources.CollectionChanged += Resources_CollectionChanged;
            parts.Recipes.CollectionChanged += Recipes_CollectionChanged;
            parts.Components.CollectionChanged += Components_CollectionChanged;
        }

        private void UnhookSelectedPartsCollections(ISelectedParts? parts)
        {
            if (parts == null) return;

            try { parts.Resources.CollectionChanged -= Resources_CollectionChanged; } catch { }
            try { parts.Recipes.CollectionChanged -= Recipes_CollectionChanged; } catch { }
            try { parts.Components.CollectionChanged -= Components_CollectionChanged; } catch { }
        }

        #endregion

        #region SelectedParts -> ListBox handlers

        private void Resources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null) return;
            if (_isUpdatingFromListView) return;

            try
            {
                _isUpdatingFromSelectedParts = true;
                HandleCollectionChangeToList(e);
            }
            finally
            {
                _isUpdatingFromSelectedParts = false;
            }
        }

        private void Recipes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null) return;
            if (_isUpdatingFromListView) return;

            try
            {
                _isUpdatingFromSelectedParts = true;
                HandleCollectionChangeToList(e);
            }
            finally
            {
                _isUpdatingFromSelectedParts = false;
            }
        }

        private void Components_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null) return;
            if (_isUpdatingFromListView) return;

            try
            {
                _isUpdatingFromSelectedParts = true;
                HandleCollectionChangeToList(e);
            }
            finally
            {
                _isUpdatingFromSelectedParts = false;
            }
        }

        private void HandleCollectionChangeToList(NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null) return;
            if (e == null) return;
            if (AssociatedObject.SelectedItems == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) break;
                    foreach (var item in e.NewItems.Cast<object>())
                        TrySelectInListView(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) break;
                    foreach (var item in e.OldItems.Cast<object>())
                        AssociatedObject.SelectedItems.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems.Cast<object>())
                            AssociatedObject.SelectedItems.Remove(item);
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems.Cast<object>())
                            TrySelectInListView(item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Remove items of the corresponding type from selection.
                    // We can't know which collection fired reset here, so best to clear all selection
                    AssociatedObject.SelectedItems.Clear();
                    break;
            }
        }

        #endregion

        #region ListBox -> SelectedParts sync

        // Called when SelectedItem changes (single selection) or SelectedItems collection changed (multi)
        private void SyncFromListViewToSelectedParts(NotifyCollectionChangedEventArgs? e = null)
        {
            if (SelectedParts == null) return;
            if (_isUpdatingFromSelectedParts) return;
            if (AssociatedObject == null) return;
            if (AssociatedObject.SelectedItems == null) return;

            try
            {
                _isUpdatingFromListView = true;

                // Single selection mode handling
                if (AssociatedObject.SelectionMode == SelectionMode.Single)
                {
                    var selected = AssociatedObject.SelectedItem;
                    if (selected == null)
                        SelectedParts.ClearSelection();
                    else
                    {
                        // try generic SelectSinglePart, else per-type
                        if (selected is ResourceViewModel r)
                            SelectedParts.SelectSingleResource(r);
                        else if (selected is RecipeViewModel rec)
                            SelectedParts.SelectSingleRecipe(rec);
                        else if (selected is RecipeComponentViewModel comp)
                            SelectedParts.SelectSingleComponent(comp);
                        else if (selected is IVMPart ivmPart)
                            SelectedParts.SelectSinglePart(ivmPart);
                        else
                            SelectedParts.ClearSelection();
                    }

                    return;
                }

                // Multi-selection mode
                // If we have collection change, handle added/removed quickly; otherwise sync full
                if (e != null)
                {
                    if (e.NewItems != null)
                    {
                        foreach (var added in e.NewItems.Cast<object>())
                        {
                            if (added is ResourceViewModel resource)
                            {
                                if (!SelectedParts.Resources.Contains(resource))
                                    SelectedParts.AddResourceToSelected(resource);
                            }
                            else if (added is RecipeViewModel recipe)
                            {
                                if (!SelectedParts.Recipes.Contains(recipe))
                                    SelectedParts.AddRecipeToSelected(recipe);
                            }
                            else if (added is RecipeComponentViewModel component)
                            {
                                if (!SelectedParts.Components.Contains(component))
                                    SelectedParts.AddComponentToSelected(component);
                            }
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (var removed in e.OldItems.Cast<object>())
                        {
                            if (removed is ResourceViewModel resource)
                            {
                                if (SelectedParts.Resources.Contains(resource))
                                    SelectedParts.Resources.Remove(resource);
                            }
                            else if (removed is RecipeViewModel recipe)
                            {
                                if (SelectedParts.Recipes.Contains(recipe))
                                    SelectedParts.Recipes.Remove(recipe);
                            }
                            else if (removed is RecipeComponentViewModel component)
                            {
                                if (SelectedParts.Components.Contains(component))
                                    SelectedParts.Components.Remove(component);
                            }
                        }
                    }
                }
                else
                {
                    // fallback: full resync from ListBox.SelectedItems
                    SelectedParts.ClearSelection();
                    foreach (var si in AssociatedObject.SelectedItems.Cast<object>())
                    {
                        if (si is ResourceViewModel resource)
                            SelectedParts.AddResourceToSelected(resource);
                        else if (si is RecipeViewModel recipe)
                            SelectedParts.AddRecipeToSelected(recipe);
                        else if (si is RecipeComponentViewModel comp)
                            SelectedParts.AddComponentToSelected(comp);
                    }
                }
            }
            finally
            {
                _isUpdatingFromListView = false;
            }
        }

        #endregion

        #region Helpers

        private void TrySelectInListView(object item)
        {
            if (AssociatedObject == null) return;
            if (item == null) return;
            if (AssociatedObject.SelectedItems == null) return;

            // Ensure operation on UI thread
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => TrySelectInListView(item));
                return;
            }

            // item must be present in Items collection (same instance)
            var items = AssociatedObject.ItemsSource ?? AssociatedObject.Items;
            if (items == null) return;

            // Check if instance exists in items (reference equality)
            bool contains = false;
            foreach (var it in items)
            {
                if (ReferenceEquals(it, item))
                {
                    contains = true;
                    break;
                }
            }

            if (!contains) return;

            if (!AssociatedObject.SelectedItems.Contains(item))
            {
                // if single selection, set SelectedItem
                if (AssociatedObject.SelectionMode == SelectionMode.Single)
                    AssociatedObject.SelectedItem = item;
                else
                    AssociatedObject.SelectedItems.Add(item);
            }
        }

        private void SyncAllSelectedPartsToListView()
        {
            if (AssociatedObject == null || SelectedParts == null || AssociatedObject.SelectedItems == null) return;

            try
            {
                _isUpdatingFromSelectedParts = true;

                // Ensure on UI thread
                if (!Dispatcher.UIThread.CheckAccess())
                {
                    Dispatcher.UIThread.Post(SyncAllSelectedPartsToListView);
                    return;
                }

                // Clear current selection and select items found in SelectedParts collections.
                AssociatedObject.SelectedItems.Clear();

                foreach (var r in SelectedParts.Resources)
                    TrySelectInListView(r);

                foreach (var rec in SelectedParts.Recipes)
                    TrySelectInListView(rec);

                foreach (var comp in SelectedParts.Components)
                    TrySelectInListView(comp);

                // If single selection mode and there are items selected in SelectedItems, ensure only one is set to SelectedItem
                if (AssociatedObject.SelectionMode == SelectionMode.Single)
                {
                    var first = AssociatedObject.SelectedItems.Cast<object>().FirstOrDefault();
                    AssociatedObject.SelectedItems.Clear();
                    if (first != null)
                        AssociatedObject.SelectedItem = first;
                }
            }
            finally
            {
                _isUpdatingFromSelectedParts = false;
            }
        }

        #endregion
    }
}
