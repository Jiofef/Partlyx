using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    public class CommandDispatcherUndoableComplexHelper : ICommandDispatcherComplexHelper
    {
        public CommandDispatcherUndoableComplexHelper() { }

        private List<IUndoableCommand> _savedCommands = new();
        public async Task ExcecuteAsync(IUndoableCommand command)
        {
            await command.ExecuteAsync();

            _savedCommands.Add(command);
        }

        internal ComplexUndoableCommand GetComplex()
        {
            var complex = new ComplexUndoableCommand(_savedCommands.ToArray());
            return complex;
        }

        internal void ClearComplex()
        {
            _savedCommands.Clear();
        }
    }
}
