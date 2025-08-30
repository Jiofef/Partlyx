namespace Partlyx.Services.Commands
{
    public interface ICommand 
    {
        Task ExecuteAsync();
    }

    public interface IUndoableCommand : ICommand 
    {
        Task UndoAsync();
    }
}
