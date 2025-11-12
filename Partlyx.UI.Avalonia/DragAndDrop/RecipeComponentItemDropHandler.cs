using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class RecipeComponentItemDropHandler : DropHandlerBase
    {
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            return false;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            e.DragEffects = DragDropEffects.None;
            return false;
        }
    }
}
