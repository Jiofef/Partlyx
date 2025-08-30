using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Partlyx.Services.Commands
{
    /// <summary>
    /// This class should reduce the same code for the commands whose task it is to set new values, and restore old ones.
    /// </summary>
    public abstract class SetValueUndoableCommand<TValue> : IUndoableCommand
    {
        private readonly TValue? _value;
        private readonly TValue? _savedValue;
        // The setter is a callback that provides a saved or new TValue depending on the action - do, undo or redo.
        private readonly Func<TValue?, Task> _setter;

        protected SetValueUndoableCommand(TValue? value, TValue? savedValue, Func<TValue?, Task> setter)
        {
            _value = value;
            _savedValue = savedValue;
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        public async Task ExecuteAsync()
        {
            await _setter(_value);
        }

        public async Task UndoAsync()
        {
            await _setter(_savedValue);
        }
    }

    // Below is an example of using SetValueUndoableCommand to create a command.

    //public class SetSomeValueCommand : SetValueUndoableCommand<object>
    //{
    //    private SetSomeValueCommand(object value, object savedValue, Func<object, Task> setter)
    //        : base(value, savedValue, setter) { }
    //
    //    public static SetSomeValueCommand Create(IServiceProvider serviceProvider, Guid resourceUid, Guid recipeUid, double amount)
    //    {
    //        var someObject = GetSomeObject();
    //
    //        return new SetSomeValueCommand(amount, someObject.GetValue(), (value) =>
    //        {
    //            someObject.SetValueAsync(resourceUid, recipeUid, value);
    //            return Task.CompletedTask;
    //        });
    //    }
    //}


    // Here is an async example

    //public class SetSomeValueCommand : SetValueUndoableCommand<object>
    //{
    //    private SetSomeValueCommand(object value, object savedValue, Func<object, Task> setter)
    //        : base(value, savedValue, setter) { }
    //
    //    public static async Task<SetSomeValueCommand?> CreateAsync(IServiceProvider serviceProvider, Guid resourceUid, Guid recipeUid, double amount)
    //    {
    //        var someObject = GetSomeObject();
    //
    //        return new SetSomeValueCommand(amount, someObject.GetValue(), async (value) =>
    //        {
    //            await someObject.SetValueAsync(resourceUid, recipeUid, value);
    //        });
    //    }
    //}
}