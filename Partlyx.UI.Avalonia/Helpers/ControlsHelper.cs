using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Partlyx.UI.Avalonia.Helpers
{
    public static class ControlsHelper
    {
        public static bool IsSourceInteractive(object? source, Visual? container)
        {
            if (source is not Visual visualSource || container == null)
                return false;

            var current = visualSource;

            while (current != null && current != container)
            {
                if (current is Control control && InteractionMarkers.GetIsPriorityClick(control))
                {
                    return true;
                }

                if (current is Button ||
                    current is ToggleButton ||
                    current is TextBox ||
                    current is CheckBox ||
                    current is ScrollBar)
                {
                    return true;
                }

                current = current.GetVisualParent();
            }

            return false;
        }
    }
}
