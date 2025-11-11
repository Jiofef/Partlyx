using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System;
using Avalonia.Input;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TextBoxFocusAndSelectBehavior : Behavior<TextBox>
    {
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<TextBoxFocusAndSelectBehavior, bool>(nameof(IsActive));

        /// <summary>
        /// If true, Focus() + selectAll() will be called when switching to true.
        /// </summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly StyledProperty<DispatcherPriority> PriorityProperty =
            AvaloniaProperty.Register<TextBoxFocusAndSelectBehavior, DispatcherPriority>(nameof(Priority), DispatcherPriority.Render);

        public DispatcherPriority Priority
        {
            get => GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            // We monitor the change of the IsActive property inside the behavior
            this.GetObservable(IsActiveProperty).Subscribe(OnIsActiveChanged);

            if (IsActive)
                PostFocusSelect();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void OnIsActiveChanged(bool value)
        {
            if (value)
                PostFocusSelect();
            else
                TransferFocusToParent();
        }

        private void PostFocusSelect()
        {
            var tb = AssociatedObject;
            if (tb == null) return;

            // Delaying so that the TextBox is already visible in the tree
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (!tb.IsVisible) return;
                    tb.Focus();
                    tb.SelectAll();
                }
                catch
                {
                    // silent
                }
            }, Priority);
        }

        private void TransferFocusToParent()
        {
            var tb = AssociatedObject;
            if (tb == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                var parent = tb.Parent as Control;
                bool focusSet = false;

                // Traverse up the logical tree to find the nearest Focusable parent.
                while (parent != null)
                {
                    // Check if the parent is focusable and effectively visible.
                    if (parent.Focusable && parent.IsEffectivelyVisible)
                    {
                        // Set focus on the parent.
                        parent.Focus();
                        focusSet = true;
                        break;
                    }
                    parent = parent.Parent as Control;
                }

                // If no Focusable parent was found, clear the focus completely.
                if (!focusSet)
                {
                    TopLevel.GetTopLevel(tb)?.FocusManager?.ClearFocus();
                }
            }, DispatcherPriority.Background); // Use low priority for reliable execution after visibility changes
        }
    }
}