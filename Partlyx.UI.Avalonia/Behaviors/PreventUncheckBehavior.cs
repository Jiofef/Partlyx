using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Avalonia.Interactivity;

namespace Partlyx.UI.Avalonia.Behaviors
{
    /// <summary>
    /// Prevents unchecking the ToggleButton by user interaction when it is already checked.
    /// Allows other controls to be clicked (so other ToggleButtons can be checked).
    /// </summary>
    public class PreventUncheckBehavior : Behavior<ToggleButton>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject?.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject?.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var button = AssociatedObject;
            if (button is { IsChecked: true })
            {
                // prevent uncheck
                e.Handled = true;
            }
        }
    }
}
