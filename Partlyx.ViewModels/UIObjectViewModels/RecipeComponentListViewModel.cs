using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class RecipeComponentListViewModel : ObservableObject, IDisposable
    {
        private readonly IVMPartsFactory _partsFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;

        public IGlobalSelectedParts SelectedParts { get; }

        public ObservableCollection<RecipeComponentItemViewModel> Components { get; } = new();

        public RecipeComponentListViewModel(IEventBus bus, IVMPartsFactory vmpf, ICommandFactory cf, ICommandDispatcher cd, IGlobalSelectedParts sp)
        {
            _partsFactory = vmpf;
            _commandFactory = cf;
            _commandDispatcher = cd;
            SelectedParts = sp;

            _childAddSubscription = bus.Subscribe<RecipeComponentCreatedEvent>(OnComponentCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeComponentDeletedEvent>(OnComponentDeleted, true);

            Components = new ObservableCollection<RecipeComponentItemViewModel>();
        }

        private void AddFromDto(RecipeComponentDto dto)
        {
            var componentVM = _partsFactory.GetOrCreateRecipeComponentVM(dto);
            Components.Add(componentVM);
        }
        private void OnComponentCreated(RecipeComponentCreatedEvent ev)
        {
            AddFromDto(ev.RecipeComponent);
        }

        private void OnComponentDeleted(RecipeComponentDeletedEvent ev)
        {
            var componentVM = Components.FirstOrDefault(c => c.Uid == ev.RecipeComponentUid);
            if (componentVM != null)
            {
                Components.Remove(componentVM);
                componentVM.Dispose();
            }
        }

        public void OnComponentsBulkLoaded(RecipeComponentsBulkLoadedEvent ev)
        {
            Components.Clear();

            foreach (var dto in ev.Bulk)
                AddFromDto(dto);
        }

        public void Dispose()
        {
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
        }

        [RelayCommand]
        private async Task CreateComponentAsync()
        {
            var parent = SelectedParts.GetSingleRecipeOrNull();
            if (parent == null)
                throw new InvalidOperationException("Create command shouldn't be called when created part's parent isn't selected");

            var command = _commandFactory.Create<CreateRecipeComponentCommand>(parent.ParentResource!, parent.Uid);
            await _commandDispatcher.ExcecuteAsync(command);
        }
    }
}
