using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsSelectionWindowViewModel<TPart> : ObservableObject, IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new();

        public ISelectedParts SelectedParts { get; }

        public PartsSelectionWindowViewModel(ISelectedParts sl)
        {
            SelectedParts = sl;

            var partsSelectedChangedSubscription = this.WhenAnyValue(t => t.SelectedParts.IsPartsSelected).Subscribe((args) => OnPropertyChanged(nameof(AllowConfirmSelection)));
            _subscriptions.Add(partsSelectedChangedSubscription);
        }

        private bool _enableMultiSelect = true;
        public bool EnableMultiSelect { get => _enableMultiSelect; set => SetProperty(ref _enableMultiSelect, value); }

        public bool AllowConfirmSelection { get => GetAllowConfirmSelection(); }

        private bool GetAllowConfirmSelection()
        {
            if (IsSelectionNecessaryToConfirm)
            {
                return SelectedParts.IsPartsSelected;
            }
            else
            {
                return true;
            }
        }

        private bool _isSelectionNecessaryToConfirm = true;
        public bool IsSelectionNecessaryToConfirm { get => _isSelectionNecessaryToConfirm; set { SetProperty(ref _isSelectionNecessaryToConfirm, value); OnPropertyChanged(nameof(AllowConfirmSelection)); } }

        private ObservableCollection<TPart>? _items;
        public ObservableCollection<TPart>? Items { get => _items; set => SetProperty(ref _items, value); }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
