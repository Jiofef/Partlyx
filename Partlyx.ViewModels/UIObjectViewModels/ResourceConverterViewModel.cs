using CommunityToolkit.Mvvm.Input;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ResourceConverterViewModel : PartlyxObservable
    {
        private readonly IDialogService _dialogService;
        private readonly IVMPartsStore _store;
        public ResourceConverterViewModel(IDialogService dialogService, IVMPartsStore store)
        {
            _dialogService = dialogService;
            _store = store;
        }
        // <-- Converting data -->
        private ResourceViewModel? _inputResource;
        public ResourceViewModel? InputResource { get => _inputResource; set => SetProperty(ref _inputResource, value); }

        private ResourceViewModel? _outputResource;
        public ResourceViewModel? OutputResource { get => _outputResource; set => SetProperty(ref _outputResource, value); }

        private double _inputAmount = 1.0;
        public double InputAmount { get => _inputAmount; set => _inputAmount = value; }
        private double _outputAmount = 1.0;

        public double OutputAmount { get => _outputAmount; set => _outputAmount = value; }

        [RelayCommand]
        public async Task SelectInput()
        {
            var selected = await SelectResource();

            if (!selected.IsCancelled)
                InputResource = selected.Resource;
        }
        [RelayCommand]
        public async Task SelectOutput()
        {
            var selected = await SelectResource();

            if (!selected.IsCancelled)
                OutputResource = selected.Resource;
        }

        private async Task<(ResourceViewModel? Resource, bool IsCancelled)> SelectResource()
        {
            var allTheResourcesList = _store.Resources.Values.ToList();
            var dialogVM = new ResourcesSelectionViewModel(_dialogService, new IsolatedSelectedParts())
            {
                EnableMultiSelect = false,
                Items = new ObservableCollection<ResourceViewModel>(allTheResourcesList),
                IsSelectionNecessaryToConfirm = true
            };

            var result = await _dialogService.ShowDialogAsync(dialogVM);

            if (result is not ISelectedParts selected) return (null, true);
            var resource = selected.GetSingleResourceOrNull()!;

            return (resource, false);
        }

        // <-- Converting options -->
        private bool _isCalculatingFromInput = true;
        public bool IsCalculatingFromInput { get => _isCalculatingFromInput; set => SetProperty(ref _isCalculatingFromInput, value); }
        private bool _isCalculatingFromOutput = false;
        public bool IsCalculatingFromOutput { get => _isCalculatingFromOutput; set => SetProperty(ref _isCalculatingFromOutput, value); }
        // <-- Converting results -->
        public ObservableCollection<RecipeComponentPathItem> AvailableConversions { get; set; } = new();
    }
}
