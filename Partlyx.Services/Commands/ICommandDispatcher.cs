

namespace Partlyx.Services.Commands
{
    public interface ICommandDispatcher
    {
        int MaxHistoryLength { get; set; }

        Task ExcecuteAsync(ICommand command);
        Task ExcecuteComplexAsync(Func<ICommandDispatcherComplexHelper, Task> complex);
        Task RedoAsync();
        Task UndoAsync();
    }
}