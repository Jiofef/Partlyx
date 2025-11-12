using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.DragAndDrop.Implementations
{
    public partial class SinglePartContainerDropHandlerViewModel<TPart> : DropHandlerBaseViewModel where TPart : IVMPart
    {
        [ObservableProperty]
        private TPart? _part;
        public override bool Validate(object? dropped, DragAndDropOptionsViewModel options)
        {
            if (dropped is TPart)
            {
                options.DragEffects = DragEffectsEnumViewModel.Copy;
                return true;
            }

            options.DragEffects = DragEffectsEnumViewModel.None;
            return false;
        }
        public override bool Drop(object? dropped)
        {
            if (dropped is TPart part)
            {
                Part = part;
                return true;
            }

            return false;
        }
    }

    public class SingleResourceContainerDropHandlerViewModel : SinglePartContainerDropHandlerViewModel<ResourceViewModel> { }
    public class SingleRecipeContainerDropHandlerViewModel : SinglePartContainerDropHandlerViewModel<RecipeViewModel> { }
    public class SingleRecipeComponentContainerDropHandlerViewModel : SinglePartContainerDropHandlerViewModel<RecipeComponentViewModel> { }
    public class SingleAnyPartContainerDropHandlerViewModel : SinglePartContainerDropHandlerViewModel<IVMPart> { }
}
