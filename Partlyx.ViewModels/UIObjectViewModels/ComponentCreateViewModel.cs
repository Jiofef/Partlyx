using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ComponentCreateViewModel : PartlyxObservable
    {
        private readonly IDialogService _dialogService;

        public IIsolatedSelectedParts SelectedParts { get; }
        public ObservableCollection<object> SelectedPartsCollection { get; } = new();
        public IResourceSearchService Search { get; }
        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;

        public ComponentCreateViewModel(IIsolatedSelectedParts isl, IResourceSearchService rss, IDialogService ds)
        {
            _dialogService = ds;

            SelectedParts = isl;
            Search = rss;

            var selectionObserver = new SelectedPartsObserveHelper(isl, SelectedPartsCollection);
            Disposables.Add(selectionObserver);
        }

        [RelayCommand]
        public void CloseDialog(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }
    }
}
