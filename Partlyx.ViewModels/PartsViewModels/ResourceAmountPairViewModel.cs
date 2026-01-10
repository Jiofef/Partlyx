using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class ResourceAmountPairViewModel : PartlyxObservable
    {
        public ResourceAmountPairViewModel(ResourceViewModel resource, double amount)
        {
            _resource = resource;
            _amount = amount;
        }

        private ResourceViewModel _resource;
        public ResourceViewModel Resource { get => _resource; set => SetProperty(ref _resource, value); }
        private double _amount;
        public double Amount { get => _amount; set => SetProperty(ref _amount, value); }
    }
}
