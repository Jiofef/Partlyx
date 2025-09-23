
namespace Partlyx.Services.Commands
{
    public interface ICommandDispatcherComplexHelper
    {
        Task ExcecuteAsync(IUndoableCommand command);
    }
}
