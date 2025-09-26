using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Partlyx.UI.WPF.Behaviors
{
    /// <summary>
    /// Prevents unchecking the ToggleButton by user interaction when it is already checked.
    /// Allows other controls to be clicked (so other ToggleButtons can be checked).
    /// </summary>
    public class PreventUncheckBehavior : Behavior<ToggleButton>
    {
        public bool PreventUncheck { get; set; } = true;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                AssociatedObject.PreviewTouchDown += OnPreviewTouchDown;
                AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                AssociatedObject.PreviewTouchDown -= OnPreviewTouchDown;
                AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            }
            base.OnDetaching();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!PreventUncheck) return;
            if (AssociatedObject == null) return;

            if (AssociatedObject.IsChecked == true)
            {
                e.Handled = true;
            }
        }

        private void OnPreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (!PreventUncheck) return;
            if (AssociatedObject == null) return;
            if (AssociatedObject.IsChecked == true)
            {
                e.Handled = true;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!PreventUncheck) return;
            if (AssociatedObject == null) return;

            if ((e.Key == Key.Space || e.Key == Key.Enter) && AssociatedObject.IsChecked == true)
            {
                e.Handled = true;
            }
        }
    }
}
