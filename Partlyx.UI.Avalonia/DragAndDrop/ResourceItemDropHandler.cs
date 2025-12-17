using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.DragAndDrop
{
    public class ResourceItemDropHandler : DropHandlerBase
    {
        // At the moment, moving items is working with bugs. It is planned to return in the future versions

        //public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        //{
        //    if (sourceContext is ITypedVMPartHolder<RecipeViewModel>)
        //    {
        //        e.DragEffects = DragDropEffects.Move;
        //        return true;
        //    }
        //    e.DragEffects = DragDropEffects.None;
        //    return false;
        //}

        //public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
        //{
        //    if (sourceContext is ITypedVMPartHolder<RecipeViewModel> recipeHolder && targetContext is ITypedVMPartHolder<ResourceViewModel> resourceHolder
        //        && resourceHolder.Part is ResourceViewModel resource)
        //    {
        //        var droppedParts = resource.GlobalNavigations.SelectedParts;
        //        Task.Run(async () =>
        //            await resource.UiItem.HandleDrop(droppedParts));
        //        return true;
        //    }

        //    return false;
        //}
    }
}
