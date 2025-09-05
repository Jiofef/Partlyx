namespace Partlyx.Services.Commands
{
    public interface ICommand 
    {
        Task ExecuteAsync();
    }

    public interface IUndoableCommand : ICommand 
    {
        Task UndoAsync();

        async Task RedoAsync() { await ExecuteAsync(); }
    }

    public interface IAsyncInitializable
    {
        Task InitializeAsync(params object[] args);
    }
}
