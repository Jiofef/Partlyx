using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class ResourceItemDropHandler : DropHandlerBase
    {
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (sourceContext is RecipeViewModel)
            {
                e.DragEffects = DragDropEffects.Move;
                return true;
            }
            e.DragEffects = DragDropEffects.None;
            return false;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (sourceContext is RecipeViewModel recipe && targetContext is ResourceViewModel resource)
            {
                var droppedParts = resource.GlobalNavigations.SelectedParts;
                Task.Run(async () =>
                    await resource.UiItem.HandleDrop(droppedParts));
                return true;
            }

            return false;
        }
    }
}
