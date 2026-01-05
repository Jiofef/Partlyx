using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    public class ComplexUndoableCommand : IUndoableCommand
    {
        private List<IUndoableCommand> _commands = new List<IUndoableCommand>();

        public ComplexUndoableCommand(params IUndoableCommand[] commands) 
        {
            _commands = commands.ToList();
        }

        public async Task ExecuteAsync()
        {
            foreach (var command in _commands)
            {
                await command.ExecuteAsync();
            }
        }

        public void AddToComplex(IUndoableCommand command)
        {
            _commands.Add(command);
        }

        public async Task UndoAsync()
        {
            // We undo the command backwards to ensure that the large operation is correctly canceled
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                var command = _commands[i];
                await command.UndoAsync();
            }
        }

        public async Task RedoAsync()
        {
            foreach (var command in _commands)
            {
                await command.RedoAsync();
            }
        }
    }
}
