using CommunityToolkit.Mvvm.ComponentModel;

namespace Partlyx.ViewModels
{
    public class PartlyxObservable : ObservableObject, IDisposable
    {
        protected List<IDisposable> Disposables { get; } = new();
        public PartlyxObservable() { }
        public void Dispose()
        {
            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
