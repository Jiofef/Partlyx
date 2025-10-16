using System.Windows;

namespace Partlyx.UI.WPF.Helpers
{
    public static class SelectionIgnoreHelper
    {
        public static readonly DependencyProperty IgnoreSelectionBehaviorProperty =
            DependencyProperty.RegisterAttached(
                "IgnoreSelectionBehavior",
                typeof(bool),
                typeof(SelectionIgnoreHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetIgnoreSelectionBehavior(DependencyObject element, bool value) =>
            element.SetValue(IgnoreSelectionBehaviorProperty, value);

        public static bool GetIgnoreSelectionBehavior(DependencyObject element) =>
            (bool)element.GetValue(IgnoreSelectionBehaviorProperty);
    }
}
