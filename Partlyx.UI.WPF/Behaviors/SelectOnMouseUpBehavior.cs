using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Partlyx.UI.WPF.Behaviors
{
    public class SelectOnMouseUpBehavior : Behavior<ListView>
    {
        // pixel distance after which we think drag started
        public double DragThreshold { get; set; } = 4.0;

        private Point _startPoint;
        private bool _isMouseDown;
        private bool _isDragging;
        private ListViewItem? _pressedItem;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject == null) return;

            _startPoint = e.GetPosition(AssociatedObject);
            _isMouseDown = true;
            _isDragging = false;

            // Find the ListViewItem below mouse pointer
            _pressedItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

            // Prevent default selection on MouseDown
            // (undoing further processing will result in SelectedItem not being changed to MouseDown)
            if (_pressedItem != null)
            {
                e.Handled = true;
                // Set focus on ListView to make keyboard selection work
                AssociatedObject.Focus();
            }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown || _pressedItem == null) return;

            var pos = e.GetPosition(AssociatedObject);
            var dx = Math.Abs(pos.X - _startPoint.X);
            var dy = Math.Abs(pos.Y - _startPoint.Y);

            if (!_isDragging && (dx > DragThreshold || dy > DragThreshold))
            {
                _isDragging = true;
                // Do not start DragDrop here - GongSolutions/other logic can start drag and drop.
                // But we will note that there was a drag.
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject == null) return;

            // If it’s not on an element - do nothing
            if (!_isMouseDown)
            {
                ResetState();
                return;
            }

            // If there was no drag'a - make the choice (given Ctrl/Shift)
            if (!_isDragging && _pressedItem != null)
            {
                // Define model element
                var item = AssociatedObject.ItemContainerGenerator.ItemFromContainer(_pressedItem);
                if (item != DependencyProperty.UnsetValue)
                {
                    HandleSelectionOnMouseUp(item);
                }
            }

            ResetState();
        }

        private void HandleSelectionOnMouseUp(object item)
        {
            if (AssociatedObject == null) return;

            var mode = AssociatedObject.SelectionMode;

            if (mode == SelectionMode.Multiple || mode == SelectionMode.Extended)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    // Toggle
                    if (AssociatedObject.SelectedItems.Contains(item))
                        AssociatedObject.SelectedItems.Remove(item);
                    else
                        AssociatedObject.SelectedItems.Add(item);
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    if (AssociatedObject.SelectedIndex >= 0)
                    {
                        var start = AssociatedObject.SelectedIndex;
                        var end = AssociatedObject.Items.IndexOf(item);
                        if (end < 0)
                        {
                            AssociatedObject.SelectedItem = item;
                            return;
                        }

                        if (start > end) (start, end) = (end, start);
                        AssociatedObject.SelectedItems.Clear();
                        for (int i = start; i <= end; i++)
                        {
                            AssociatedObject.SelectedItems.Add(AssociatedObject.Items[i]);
                        }
                    }
                    else
                    {
                        AssociatedObject.SelectedItem = item;
                    }
                }
                else
                {
                    // Simple single selection - replace selection
                    AssociatedObject.SelectedItems.Clear();
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
            else
            {
                // Single selection
                AssociatedObject.SelectedItem = item;
            }
        }

        private void ResetState()
        {
            _isMouseDown = false;
            _isDragging = false;
            _pressedItem = null;
        }

        // Helper function: go up the tree and find ListViewItem
        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T typed) return typed;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
