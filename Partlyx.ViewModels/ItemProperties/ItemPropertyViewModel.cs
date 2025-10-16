using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.PartsViewModels;
using System.Windows.Input;

namespace Partlyx.ViewModels.ItemProperties
{
    public partial class ItemPropertyViewModel : ObservableObject, IDisposable
    {
        public List<IDisposable> Subscriptions { get; } = new();

        private object? _item;
        public object? Item { get => _item; set => SetProperty(ref _item, value); }

        private string _name = "";
        public string Name { get => _name; set { SetProperty(ref _name, value); OnPropertyChanged(nameof(DisplayableName)); } }

        public string DisplayableName { get => _name + ":"; }

        public Func<object?, Task>? SaveChangesTask { get; set; }

        [RelayCommand]
        public async Task SaveChanges(object? args)
        {
            if (SaveChangesTask != null)
                await SaveChangesTask(args);
        }

        public virtual void Dispose()
        {
            foreach (var subscription in Subscriptions)
                subscription.Dispose();
        }
    }
}
