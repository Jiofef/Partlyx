using Partlyx.Services.Commands;

namespace Partlyx.Services.OtherEvents
{
    public record CommandExcecutedEvent(ICommand Command);
    public record CommandRedoedEvent(ICommand Command);
    public record CommandUndoedEvent(ICommand Command, ICommand? PreviousCommand);

    public record FileSavedEvent();
}
