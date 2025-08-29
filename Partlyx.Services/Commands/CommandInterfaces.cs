namespace Partlyx.Services.Commands
{
    public interface ICommand 
    {
        Task ExcecuteAsync();
    }

    public interface IUndoableCommand : ICommand 
    {
        Task UndoAsync();
    }
}
