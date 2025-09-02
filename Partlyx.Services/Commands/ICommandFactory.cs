
namespace Partlyx.Services.Commands
{
    public interface ICommandFactory
    {
        T Create<T>(params object[] args) where T : ICommand;
        Task<T> CreateAsync<T>(params object[] args) where T : class, ICommand, IAsyncInitializable;
    }
}
