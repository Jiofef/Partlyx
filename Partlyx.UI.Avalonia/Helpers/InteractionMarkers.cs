using Avalonia;
using Avalonia.Controls;

namespace Partlyx.UI.Avalonia.Helpers
{
    public class InteractionMarkers
    {
        public static readonly AttachedProperty<bool> IsPriorityClickProperty =
            AvaloniaProperty.RegisterAttached<InteractionMarkers, Control, bool>(
                "IsPriorityClick",
                defaultValue: false,
                inherits: true);

        public static void SetIsPriorityClick(Control element, bool value)
        {
            element.SetValue(IsPriorityClickProperty, value);
        }

        public static bool GetIsPriorityClick(Control element)
        {
            return element.GetValue(IsPriorityClickProperty);
        }
    }
}
