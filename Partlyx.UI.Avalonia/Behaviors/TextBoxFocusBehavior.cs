using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Partlyx.UI.Avalonia.Helpers;
using System;
using System.Collections.Generic;


namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TextBoxFocusBehavior : Behavior<TextBox>
    {
        public static readonly StyledProperty<bool> IsFocusedProperty =
            AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(
                nameof(IsFocused),
                defaultValue: false);

        public static readonly StyledProperty<bool> SelectAllOnFocusProperty =
            AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(
                nameof(SelectAllOnFocus),
                defaultValue: false);

        public bool IsFocused
        {
            get => GetValue(IsFocusedProperty);
            set => SetValue(IsFocusedProperty, value);
        }

        public bool SelectAllOnFocus
        {
            get => GetValue(SelectAllOnFocusProperty);
            set => SetValue(SelectAllOnFocusProperty, value);
        }

        private void OnIsFocusedChanged()
        {
            FocusTextBox();
        }

        private readonly List<IDisposable> _subscriptions = new();
        protected override void OnAttached()
        {
            base.OnAttached();

            var isFocusedPropertySubscription = AssociatedObject?.Observe(IsFocusedProperty, _ => OnIsFocusedChanged());
            if (isFocusedPropertySubscription != null)
                _subscriptions.Add(isFocusedPropertySubscription);

            if (IsFocused)
                FocusTextBox();
        }

        private void FocusTextBox()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (AssociatedObject == null) return;

                AssociatedObject.Focus();

                if (SelectAllOnFocus)
                {
                    AssociatedObject.SelectAll();
                }
                else
                {
                    AssociatedObject.CaretIndex = AssociatedObject.Text?.Length ?? 0;
                }
            }, DispatcherPriority.Input);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
                _subscriptions.Clear();
            }
        }
    }
}
