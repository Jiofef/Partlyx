
namespace Partlyx.Services.Commands
{
    public interface ICommandDispatcher
    {
        int MaxHistoryLength { get; set; }

        Task ExcecuteAsync(ICommand command);
        Task RedoAsync();
        Task UndoAsync();
    }
}