using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.UI.WPF.Behaviors
{
    public class ListViewSelectedPartsSyncBehavior : Behavior<ListView>
    {
        #region SelectedParts DP
        public static readonly DependencyProperty SelectedPartsProperty =
            DependencyProperty.Register(
                nameof(SelectedParts),
                typeof(ISelectedParts),
                typeof(ListViewSelectedPartsSyncBehavior),
                new PropertyMetadata(null, OnSelectedPartsChanged));

        public ISelectedParts SelectedParts
        {
            get => (ISelectedParts)GetValue(SelectedPartsProperty);
            set => SetValue(SelectedPartsProperty, value);
        }

        private static void OnSelectedPartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var beh = (ListViewSelectedPartsSyncBehavior)d;
            beh.UnhookSelectedPartsCollections(e.OldValue as ISelectedParts);
            beh.HookSelectedPartsCollections(e.NewValue as ISelectedParts);
            // sync initial state from SelectedParts to ListView
            beh.SyncAllSelectedPartsToListView();
        }
        #endregion

        private bool _isUpdatingFromSelectedParts = false;
        private bool _isUpdatingFromListView = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            }

            if (SelectedParts != null)
            {
                HookSelectedPartsCollections(SelectedParts);
                SyncAllSelectedPartsToListView();
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            }
            UnhookSelectedPartsCollections(SelectedParts);
            base.OnDetaching();
        }

        private void AssociatedObject_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (SelectedParts == null) return;
            if (_isUpdatingFromSelectedParts) return;

            try
            {
                _isUpdatingFromListView = true;

                // Single selection mode
                if (AssociatedObject.SelectionMode == SelectionMode.Single)
                {
                    var selected = AssociatedObject.SelectedItem;
                    if (selected == null)
                    {
                        SelectedParts.ClearSelection();
                    }
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

                // Multiple selection mode
                // Added items => add to SelectedParts (if not exist)
                foreach (var added in e.AddedItems.Cast<object>())
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

                // Removed items => remove from SelectedParts collections
                foreach (var removed in e.RemovedItems.Cast<object>())
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
            finally
            {
                _isUpdatingFromListView = false;
            }
        }

        #region Hook / Unhook collections
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

            parts.Resources.CollectionChanged -= Resources_CollectionChanged;
            parts.Recipes.CollectionChanged -= Recipes_CollectionChanged;
            parts.Components.CollectionChanged -= Components_CollectionChanged;
        }
        #endregion

        #region CollectionChanged handlers (SelectedParts -> ListView)
        private void Resources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null) return;
            if (_isUpdatingFromListView) return;

            try
            {
                _isUpdatingFromSelectedParts = true;

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
                        // Clear all resource selections from ListView
                        RemoveItemsOfTypeFromSelection<ResourceViewModel>();
                        break;
                }
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
                        RemoveItemsOfTypeFromSelection<RecipeViewModel>();
                        break;
                }
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
                        RemoveItemsOfTypeFromSelection<RecipeComponentViewModel>();
                        break;
                }
            }
            finally
            {
                _isUpdatingFromSelectedParts = false;
            }
        }
        #endregion

        #region Helpers
        private void TrySelectInListView(object item)
        {
            if (AssociatedObject == null) return;
            if (item == null) return;

            // item must be present in Items collection (same instance)
            if (AssociatedObject.Items.Contains(item))
            {
                if (!AssociatedObject.SelectedItems.Contains(item))
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
        }

        private void RemoveItemsOfTypeFromSelection<TItem>() where TItem : class
        {
            if (AssociatedObject == null) return;
            // remove selected items that are of TItem
            var toRemove = AssociatedObject.SelectedItems.Cast<object>().Where(x => x is TItem).ToList();
            foreach (var r in toRemove)
                AssociatedObject.SelectedItems.Remove(r);
        }

        private void SyncAllSelectedPartsToListView()
        {
            if (AssociatedObject == null || SelectedParts == null) return;

            try
            {
                _isUpdatingFromSelectedParts = true;

                // Clear current selection and select items found in SelectedParts collections.
                AssociatedObject.SelectedItems.Clear();

                foreach (var r in SelectedParts.Resources)
                {
                    TrySelectInListView(r);
                }
                foreach (var rec in SelectedParts.Recipes)
                {
                    TrySelectInListView(rec);
                }
                foreach (var comp in SelectedParts.Components)
                {
                    TrySelectInListView(comp);
                }

                // If ListView is Single and there's any selected part, ensure only one is set
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
