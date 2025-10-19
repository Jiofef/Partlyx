using Partlyx.Core;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.OtherEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Partlyx.Services.Commands
{
    public class CommandDispatcher : ICommandDispatcher, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly CommandDispatcherUndoableComplexHelper _complexHelper;

        private readonly IDisposable _fileClosedSubscription;

        public CommandDispatcher(IEventBus bus)
        {
            _bus = bus;
            _complexHelper = new CommandDispatcherUndoableComplexHelper();

            _fileClosedSubscription = _bus.Subscribe<FileClosedEvent>(ev => ClearHistory());
        }

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
            OnCommandExcecuted(command);
        }

        public async Task ExcecuteComplexAsync(Func<ICommandDispatcherComplexHelper, Task> complexAction)
        {
            await complexAction(_complexHelper);
            var complex = _complexHelper.GetComplex();
            _complexHelper.ClearComplex();
            OnCommandExcecuted(complex);
        }

        public async Task UndoAsync()
        {
            if (_commandsHistory.Count == 0) return;

            IUndoableCommand cancelledCommand = _commandsHistory.Last!.Value;
            _commandsHistory.RemoveLast();

            await cancelledCommand.UndoAsync();
            _canceledCommandsHistory.AddLast(cancelledCommand);

            var previousCommand = _commandsHistory.Count > 0 ? _commandsHistory.Last.Value : null;
            _bus.Publish(new CommandUndoedEvent(cancelledCommand, previousCommand));
        }
        public async Task RedoAsync()
        {
            if (_canceledCommandsHistory.Count == 0) return;

            var command = _canceledCommandsHistory.Last!.Value;
            _canceledCommandsHistory.RemoveLast();

            await command.RedoAsync();

            _commandsHistory.AddLast(command);

            if (_commandsHistory.Count > MaxHistoryLength)
                _commandsHistory.RemoveFirst();

            _bus.Publish(new CommandRedoedEvent(command));
        }

        private void OnCommandExcecuted(ICommand command)
        {
            _bus.Publish(new CommandExcecutedEvent(command));
            if (command is IUndoableCommand uCommand)
            {
                _commandsHistory.AddLast(uCommand);

                if (_commandsHistory.Count > MaxHistoryLength)
                    _commandsHistory.RemoveFirst();
            }
            _canceledCommandsHistory.Clear();
        }

        private void ClearHistory()
        {
            _commandsHistory.Clear();
            _canceledCommandsHistory.Clear();
        }

        public void Dispose()
        {
            _fileClosedSubscription.Dispose();
        }
    }
}
