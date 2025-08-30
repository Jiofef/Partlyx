using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    public class CommandDispatcher
    {
        private int maxHistoryLength = 100;
        public int MaxHistoryLength 
        { 
            get => maxHistoryLength;
            set 
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

                maxHistoryLength = value;
                if (_commandsHistory.Count > maxHistoryLength)
                    TrimHistoryToMax();
            }
        }

        private readonly LinkedList<IUndoableCommand> _commandsHistory = new();
        private readonly LinkedList<IUndoableCommand> _canceledCommandsHistory = new();

        private void TrimHistoryToMax()
        {
            if (maxHistoryLength == 0)
            {
                _commandsHistory.Clear();
                return;
            }

            while (_commandsHistory.Count > maxHistoryLength)
            {
                _commandsHistory.RemoveFirst();
            }
        }

        public async Task ExcecuteAsync(ICommand command)
        {
            await command.ExecuteAsync();

            if (command is IUndoableCommand uCommand)
            {
                _commandsHistory.AddLast(uCommand);

                if (_commandsHistory.Count > MaxHistoryLength)
                    _commandsHistory.RemoveFirst();
            }

            _canceledCommandsHistory.Clear();
        }

        public async Task UndoAsync()
        {
            if (_commandsHistory.Count == 0) return;

            IUndoableCommand cancelledCommand = _commandsHistory.Last!.Value;
            _commandsHistory.RemoveLast();

            await cancelledCommand.UndoAsync();
            _canceledCommandsHistory.AddLast(cancelledCommand);
        }
        public async Task RedoAsync()
        {
            if (_canceledCommandsHistory.Count == 0) return;

            var command = _canceledCommandsHistory.Last!.Value;
            _canceledCommandsHistory.RemoveLast();

            await command.ExecuteAsync();

            _commandsHistory.AddLast(command);

            if (_commandsHistory.Count > MaxHistoryLength)
                _commandsHistory.RemoveFirst();
        }
    }
}
