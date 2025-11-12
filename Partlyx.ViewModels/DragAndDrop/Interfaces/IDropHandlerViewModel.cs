namespace Partlyx.ViewModels.DragAndDrop.Interfaces
{
    public interface IDropHandlerViewModel
    {
        bool Validate(object? dropped, DragAndDropOptionsViewModel options);
        bool Drop(object? dropped);
    }
}
