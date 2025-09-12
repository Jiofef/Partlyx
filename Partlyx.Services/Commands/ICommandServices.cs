
namespace Partlyx.Services.Commands
{
    public interface ICommandServices
    {
        ICommandDispatcher Dispatcher { get; }
        ICommandFactory Factory { get; }

        Task<TCommand> CreateAsyncEndExcecuteAsync<TCommand>(params object[] args) where TCommand : class, ICommand, IAsyncInitializable;
        Task<TCommand> CreateSyncAndExcecuteAsync<TCommand>(params object[] args) where TCommand : ICommand;
    }
}