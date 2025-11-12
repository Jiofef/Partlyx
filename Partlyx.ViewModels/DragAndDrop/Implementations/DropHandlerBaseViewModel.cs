using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Partlyx.ViewModels.DragAndDrop;
using Partlyx.ViewModels.DragAndDrop.Interfaces;

namespace Partlyx.ViewModels.DragAndDrop.Implementations
{
    public abstract class DropHandlerBaseViewModel : ObservableObject, IDropHandlerViewModel
    {
        public abstract bool Validate(object? dropped, DragAndDropOptionsViewModel options);
        public abstract bool Drop(object? dropped);
    }
}
