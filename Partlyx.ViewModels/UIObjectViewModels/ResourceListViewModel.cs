using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using System.Diagnostics;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.Infrastructure.Data.CommonFileEvents;

namespace Partlyx.ViewModels
{
    public partial class ResourceListViewModel : ObservableObject, IDisposable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IVMPartsStore _store;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _initializationFinishedSubscription;
        private readonly IDisposable _fileClearedSubscription;

        public IGlobalSelectedParts SelectedParts { get; }
        public IResourceSearchService Search { get; }

        private IGlobalResourcesVMContainer _resourcesContainer { get; }
        public ObservableCollection<ResourceItemViewModel> Resources => _resourcesContainer.Resources;

        public ResourceListViewModel(IGlobalResourcesVMContainer grvmc, IGlobalSelectedParts sp, IEventBus bus, IVMPartsFactory vmpf,
                ICommandFactory cf, ICommandDispatcher cd, IResourceService rs, IVMPartsStore vmps, IResourceSearchService rss)
        {
            _resourcesContainer = grvmc;
            _partsFactory = vmpf;
            _commandFactory = cf;
            _commandDispatcher = cd;
            _store = vmps;

            SelectedParts = sp;
            Search = rss;

            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
            _initializationFinishedSubscription = bus.Subscribe<PartsVMInitializationFinishedEvent>((ev) => UpdateList(), true);
            _fileClearedSubscription = bus.Subscribe<FileClearedEvent>((ev) => Resources.Clear(), true);
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
            _initializationFinishedSubscription.Dispose();
            _fileClearedSubscription.Dispose();
        }

        public void UpdateList()
        {
            Resources.Clear();

            foreach (var resource in _store.Resources.Values)
                Resources.Add(resource);
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
