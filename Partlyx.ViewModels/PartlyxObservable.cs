using CommunityToolkit.Mvvm.ComponentModel;

namespace Partlyx.ViewModels
{
    public class PartlyxObservable : ObservableObject, IDisposable
    {
        public bool IsDisposed { get; private set; } = false;
        protected List<IDisposable> Disposables { get; } = new();
        public PartlyxObservable() { }
        public void Dispose()
        {
            if (IsDisposed) return;

            foreach (var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();

            IsDisposed = true;
        }
    }
}
