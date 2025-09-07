using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using System.Diagnostics;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.PartsEventClasses;

namespace Partlyx.ViewModels
{
    public partial class ResourceListViewModel : ObservableObject, IDisposable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;

        public ObservableCollection<ResourceItemViewModel> Resources { get; } = new();

        public ResourceListViewModel(IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd)
        {
            _partsFactory = vmpf;
            _commandFactory = cf;
            _commandDispatcher = cd;

            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);

            Resources = new ObservableCollection<ResourceItemViewModel>();
        }

        private void OnResourceCreated(ResourceCreatedEvent ev)
        {
            var resourceVM = _partsFactory.CreateResourceVM(ev.Resource);
            Resources.Add(resourceVM);
        }

        private void OnResourceDeleted(ResourceDeletedEvent ev)
        {
            var resourceVM = Resources.FirstOrDefault(c => c.Uid == ev.ResourceUid);
            if (resourceVM != null)
            {
                Resources.Remove(resourceVM);
                resourceVM.Dispose();
            }
        }

        public void Dispose()
        {
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
        }

        [RelayCommand]
        private async Task CreateResourceAsync()
        {
            var command = _commandFactory.Create<CreateResourceCommand>();
            await _commandDispatcher.ExcecuteAsync(command);
        }
    }
}
