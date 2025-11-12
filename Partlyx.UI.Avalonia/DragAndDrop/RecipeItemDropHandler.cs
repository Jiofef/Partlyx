using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class RecipeItemDropHandler : DropHandlerBase
    {
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (sourceContext is RecipeComponentViewModel)
            {
                e.DragEffects = DragDropEffects.Move;
                return true;
            }
            e.DragEffects = DragDropEffects.None;
            return false;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (sourceContext is RecipeComponentViewModel component && targetContext is RecipeViewModel recipe)
            {
                var droppedParts = recipe.GlobalNavigations.SelectedParts;
                Task.Run(async () =>
                    await recipe.UiItem.HandleDrop(droppedParts));
                return true;
            }

            return false;
        }
    }
}
