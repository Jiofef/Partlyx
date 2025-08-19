namespace Partlyx.Core
{
    public interface ICommand { }

    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task Handle(TCommand command);
    }

    public interface IQuery <TResult> { }

    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult> 
    {
        Task<TResult> Handle(TQuery query);
    }
}
