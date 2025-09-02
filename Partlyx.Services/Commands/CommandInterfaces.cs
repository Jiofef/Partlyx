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

    public interface IAsyncInitializable
    {
        Task InitializeAsync(params object[] args);
    }
}
