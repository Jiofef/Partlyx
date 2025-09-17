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
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels
{
    public partial class ResourceListViewModel : ObservableObject, IDisposable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IResourceService _resourceService;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _bulkLoadedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }

        public ObservableCollection<ResourceItemViewModel> Resources { get; } = new();

        public ResourceListViewModel(IGlobalSelectedParts sp, IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd, IResourceService rs)
        {
            _partsFactory = vmpf;
            _commandFactory = cf;
            _commandDispatcher = cd;
            _resourceService = rs;

            SelectedParts = sp;

            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
            _bulkLoadedSubscription = bus.Subscribe<ResourcesBulkLoadedEvent>(OnResourceBulkLoaded, true);

            Resources = new ObservableCollection<ResourceItemViewModel>();
        }

        private void AddFromDto(ResourceDto dto)
        {
            var resourceVM = _partsFactory.GetOrCreateResourceVM(dto);
            Resources.Add(resourceVM);
        }

        private void OnResourceCreated(ResourceCreatedEvent ev)
        {
            AddFromDto(ev.Resource);
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

        private void OnResourceBulkLoaded(ResourcesBulkLoadedEvent ev)
        {
            Resources.Clear();

            foreach (var dto in ev.Bulk)
                AddFromDto(dto);
        }

        public async Task UpdateList()
        {
            Resources.Clear();

            var resourceDtos = await _resourceService.GetAllTheResourcesAsync();

            foreach (var dto in resourceDtos)
                AddFromDto(dto);
        }

        [RelayCommand]
        private async Task CreateResourceAsync()
        {
            var command = _commandFactory.Create<CreateResourceCommand>();
            await _commandDispatcher.ExcecuteAsync(command);
        }

        [RelayCommand]
        private void StartRenamingSelected()
        {
            var resourceVM = SelectedParts.GetSingleResourceOrNull();
            if (resourceVM == null) return;

            resourceVM.Ui.IsRenaming = true;
        }
    }
}
