﻿using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIStates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public record TreeSearchQueryEvent(string queryText);

    public partial class PartsTreeViewModel
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _initializationFinishedSubscription;
        private readonly IDisposable _fileClearedSubscription;
        private readonly IDisposable _treeSearchQuerySubscription;

        public IGlobalSelectedParts SelectedParts { get; }
        public IGlobalFocusedPart FocusedPart { get; }
        public IResourceSearchService Search { get; }
        public ResourceServiceViewModel Service { get; }

        private IGlobalResourcesVMContainer _resourcesContainer { get; }
        public ObservableCollection<ResourceViewModel> Resources => _resourcesContainer.Resources;

        public PartsTreeViewModel(IGlobalResourcesVMContainer grvmc, IGlobalSelectedParts sp, IGlobalFocusedPart fp, IEventBus bus, IVMPartsFactory vmpf,
                IVMPartsStore vmps, IResourceSearchService rss, ResourceServiceViewModel service)
        {
            _resourcesContainer = grvmc;
            _partsFactory = vmpf;
            _store = vmps;
            _bus = bus;

            SelectedParts = sp;
            FocusedPart = fp;
            Search = rss;
            Service = service;

            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
            _initializationFinishedSubscription = bus.Subscribe<PartsVMInitializationFinishedEvent>((ev) => UpdateList(), true);
            _fileClearedSubscription = bus.Subscribe<FileClearedEvent>((ev) => Resources.Clear(), true);
            _treeSearchQuerySubscription = bus.Subscribe<TreeSearchQueryEvent>(SearchQueryHandler);
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
            _treeSearchQuerySubscription.Dispose();
        }

        public void UpdateList()
        {
            Resources.Clear();

            foreach (var resource in _store.Resources.Values)
                Resources.Add(resource);
        }

        private void SearchQueryHandler(TreeSearchQueryEvent ev)
        {
            Search.SearchText = ev.queryText;
        }

        [RelayCommand]
        public void CallSearch(string queryText)
        {
            Search.SearchText = queryText;
        }

        [RelayCommand]
        public void ExpandAll()
        {
            var ev = new SetAllThePartItemsExpandedEvent(true);
            _bus.Publish(ev);
        }

        [RelayCommand]
        public void CollapseAll()
        {
            var ev = new SetAllThePartItemsExpandedEvent(false);
            _bus.Publish(ev);
        }
    }
}
