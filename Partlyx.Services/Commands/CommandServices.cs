using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    /// <summary>
    /// Both container and facade for full command using cycle
    /// </summary>
    public class CommandServices : ICommandServices
    {
        public CommandServices(ICommandDispatcher cd, ICommandFactory cf)
        {
            Dispatcher = cd;
            Factory = cf;
        }
        public ICommandDispatcher Dispatcher { get; }
        public ICommandFactory Factory { get; }

        public async Task<TCommand> CreateAndExcecuteAsync<TCommand>(params object[] args) where TCommand : class, ICommand
        {
            TCommand command = await Factory.CreateAsync<TCommand>(args);
            await Dispatcher.ExcecuteAsync(command);
            return command;
        }

        public async Task<TCommand> CreateAndExcecuteInLastComplexAsync<TCommand>(params object[] args) where TCommand : class, IUndoableCommand
        {
            TCommand command = await Factory.CreateAsync<TCommand>(args);
            await Dispatcher.ExcecuteInLastComplexAsync(command);
            return command;
        }
        public async Task<TCommand> CreateAndExcecuteInLastComplexAsyncIf<TCommand>(bool executeInComplex, params object[] args) where TCommand : class, IUndoableCommand
        {
            TCommand command = await Factory.CreateAsync<TCommand>(args);
            if (executeInComplex)
                await Dispatcher.ExcecuteInLastComplexAsync(command);
            else
                await Dispatcher.ExcecuteAsync(command);
            return command;
        }
    }
}
