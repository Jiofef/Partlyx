
namespace Partlyx.Services.Commands
{
    public interface ICommandServices
    {
        ICommandDispatcher Dispatcher { get; }
        ICommandFactory Factory { get; }

        Task<TCommand> CreateAndExcecuteAsync<TCommand>(params object[] args) where TCommand : class, ICommand;
        Task<TCommand> CreateAndExcecuteInLastComplexAsync<TCommand>(params object[] args) where TCommand : class, IUndoableCommand;
        Task<TCommand> CreateAndExcecuteInLastComplexAsyncIf<TCommand>(bool executeInComplex, params object[] args) where TCommand : class, IUndoableCommand;
    }
}