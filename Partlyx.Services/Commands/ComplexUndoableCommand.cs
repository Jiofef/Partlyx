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

        public async Task UndoAsync()
        {
            foreach (var commands in _commands)
            {
                await commands.UndoAsync();
            }
        }

        public async Task RedoAsync()
        {
            foreach (var commands in _commands)
            {
                await commands.RedoAsync();
            }
        }
    }
}
