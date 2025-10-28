using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class PartsSelectionWindowViewModel<TPart> : ObservableObject, IDisposable
    {
        private readonly IDialogService _dialogService;

        private readonly List<IDisposable> _subscriptions = new();

        public ISelectedParts SelectedParts { get; }

        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;

        public PartsSelectionWindowViewModel(IDialogService ds, ISelectedParts sl)
        {
            _dialogService = ds;

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

        [RelayCommand]
        public void CloseDialog(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }

    public class ResourcesSelectionViewModel : PartsSelectionWindowViewModel<ResourceViewModel>
    {
        public ResourcesSelectionViewModel(IDialogService ds, ISelectedParts sl) : base(ds, sl) { }
    }

    public class RecipesSelectionViewModel : PartsSelectionWindowViewModel<RecipeViewModel>
    {
        public RecipesSelectionViewModel(IDialogService ds, ISelectedParts sl) : base(ds, sl) { }
    }
    public class RecipeComponentsSelectionViewModel : PartsSelectionWindowViewModel<RecipeComponentViewModel>
    {
        public RecipeComponentsSelectionViewModel(IDialogService ds, ISelectedParts sl) : base(ds, sl) { }
    }
    public class AnyPartsSelectionViewModel : PartsSelectionWindowViewModel<IVMPart>
    {
        public AnyPartsSelectionViewModel(IDialogService ds, ISelectedParts sl) : base(ds, sl) { }
    }
}
