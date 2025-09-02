using Partlyx.Services.Commands;

namespace Partlyx.Tests
{
    public class CommandDispatcherTest
    {
        [Fact]
        public async Task ExcecuteAsync_Add5ToNumber2Times_GetNumber10()
        {
            // Arrange
            CommandDispatcher dispatcher = new CommandDispatcher();
            Number number = new Number();
            NumberAdderCommand command1 = new(number, 5);
            NumberAdderCommand command2 = new(number, 5);

            // Act
            await dispatcher.ExcecuteAsync(command1);
            await dispatcher.ExcecuteAsync(command2);

            // Arrange
            Assert.Equal(10, number.Value);
        }

        [Fact]
        public async Task UndoAsync_Add7ToNumberAndUndo_GetNumber0()
        {
            // Arrange
            CommandDispatcher dispatcher = new CommandDispatcher();
            Number number = new Number();
            NumberAdderCommand command = new(number, 7);

            // Act
            await dispatcher.ExcecuteAsync(command);
            await dispatcher.UndoAsync();

            // Assert
            Assert.Equal(0, number.Value);
        }

        [Fact]
        public async Task UndoAsync_Add7ToNumber4TimesAndUndoWithHistoryLength2_GetNumber14()
        {
            // Arrange
            CommandDispatcher dispatcher = new CommandDispatcher();
            Number number = new Number();
            NumberAdderCommand command1 = new(number, 7);
            NumberAdderCommand command2 = new(number, 7);
            NumberAdderCommand command3 = new(number, 7);
            NumberAdderCommand command4 = new(number, 7);

            dispatcher.MaxHistoryLength = 2;

            // Act
            await dispatcher.ExcecuteAsync(command1);
            await dispatcher.ExcecuteAsync(command2);
            await dispatcher.ExcecuteAsync(command3);
            await dispatcher.ExcecuteAsync(command4);

            await dispatcher.UndoAsync();
            await dispatcher.UndoAsync();
            await dispatcher.UndoAsync();
            await dispatcher.UndoAsync();

            // Assert
            Assert.Equal(14, number.Value);
        }

        [Fact]
        public async Task RedoAsync_Add8ToNumberThenUndoAndRedo_GetNumber8()
        {
            // Arrange
            CommandDispatcher dispatcher = new CommandDispatcher();
            Number number = new Number();
            NumberAdderCommand command = new(number, 8);

            // Act
            await dispatcher.ExcecuteAsync(command);

            await dispatcher.UndoAsync();
            await dispatcher.RedoAsync();

            // Assert
            Assert.Equal(8, number.Value);
        }

        private class Number()
        {
            public int Value { get; set; } = 0;
        }

        private class NumberAdderCommand : IUndoableCommand
        {
            private int _valueToAdd;
            private Number _numberObject;

            public NumberAdderCommand(Number numberObject, int valueToAdd)
            {
                _numberObject = numberObject;
                _valueToAdd = valueToAdd;
            }

            public Task ExecuteAsync()
            {
                _numberObject.Value += _valueToAdd;
                return Task.CompletedTask;
            }

            public Task UndoAsync()
            {
                _numberObject.Value -= _valueToAdd;
                return Task.CompletedTask;
            }
        }
    }
}