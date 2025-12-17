using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.UI.Avalonia.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class RecipeItemDropHandler : DropHandlerBase
    {
        public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if (
                //sourceContext is ITypedVMPartHolder<RecipeComponentViewModel> ||  - At the moment, moving items is working with bugs. It is planned to return in the future versions
                sourceContext is ITypedVMPartHolder<ResourceViewModel>)
            {
                e.DragEffects = DragDropEffects.Move;
                return true;
            }
            e.DragEffects = DragDropEffects.None;
            return false;
        }

        public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        {
            if ((//sourceContext is ITypedVMPartHolder<RecipeComponentViewModel> || 
                sourceContext is ITypedVMPartHolder<ResourceViewModel>)
                && targetContext is ITypedVMPartHolder<RecipeViewModel> recipeHolder
                && recipeHolder.Part is RecipeViewModel recipe)
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
