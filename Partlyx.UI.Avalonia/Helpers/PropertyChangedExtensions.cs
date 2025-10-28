using Avalonia;
using System;

namespace Partlyx.UI.Avalonia.Helpers
{
    public static class PropertyChangedExtensions
    {
        private class ActionObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            public ActionObserver(Action<T> onNext) => _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            public void OnNext(T value) => _onNext(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        public static IDisposable Observe<T>(
            this AvaloniaObject target,
            AvaloniaProperty<T> property,
            Action<T> onChanged)
        {
            return target.GetObservable(property).Subscribe(new ActionObserver<T>(onChanged));
        }
    }

}
