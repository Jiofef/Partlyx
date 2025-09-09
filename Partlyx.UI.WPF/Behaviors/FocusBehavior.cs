using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Partlyx.UI.WPF.Behaviors
{
    public class FocusBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
                nameof(IsFocused),
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.Register(
                nameof(SelectAllOnFocus),
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false));

        public bool IsFocused
        {
            get => (bool)GetValue(IsFocusedProperty);
            set => SetValue(IsFocusedProperty, value);
        }

        public bool SelectAllOnFocus
        {
            get => (bool)GetValue(SelectAllOnFocusProperty);
            set => SetValue(SelectAllOnFocusProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (FocusBehavior)d;
            if ((bool)e.NewValue)
                behavior.FocusTextBox();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (IsFocused)
                FocusTextBox();
        }

        private void FocusTextBox()
        {
            AssociatedObject?.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (AssociatedObject == null) return;

                AssociatedObject.Focus();
                Keyboard.Focus(AssociatedObject);

                if (SelectAllOnFocus)
                {
                    AssociatedObject.SelectAll();
                }
                else
                {
                    AssociatedObject.CaretIndex = AssociatedObject.Text?.Length ?? 0;
                }
            }), DispatcherPriority.Input);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
