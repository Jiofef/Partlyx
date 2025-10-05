using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.RecipeComponentCommonCommands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeComponentItemViewModel : UpdatableViewModel<RecipeComponentDto>, IVMPart
    {
        // Servicess
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeComponentItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;
        private readonly ILinkedPartsManager _linkedPartsManager;
        public PartsServiceViewModel Services { get; }

        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;

        public RecipeComponentItemViewModel(RecipeComponentDto dto, PartsServiceViewModel service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus, IRecipeComponentItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;
            _commands = cs;
            _linkedPartsManager = lpm;

            // Info
            if (dto.ParentRecipeUid is Guid parentUid)
                LinkedParentRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(parentUid);
            LinkedResource = _linkedPartsManager.CreateAndRegisterLinkedResourceVM(dto.ResourceUid);
            _quantity = dto.Quantity;
            if (dto.SelectedRecipeUid is Guid selectedUid)
                LinkedSelectedRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(selectedUid);

            // Info updating binding
            _updatedSubscription = bus.Subscribe<RecipeComponentUpdatedEvent>(OnRecipeComponentUpdated, true);
        }

        // Recipe component info
        public Guid Uid { get; }

        private GuidLinkedPart<RecipeItemViewModel>? _parentRecipe;
        public GuidLinkedPart<RecipeItemViewModel>? LinkedParentRecipe { get => _parentRecipe; set => SetProperty(ref _parentRecipe, value); }

        private GuidLinkedPart<ResourceItemViewModel>? _resource;
        public GuidLinkedPart<ResourceItemViewModel>? LinkedResource { get => _resource; private set => SetProperty(ref _resource, value); }

        private double _quantity;
        public double Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private GuidLinkedPart<RecipeItemViewModel>? _selectedRecipe;
        public GuidLinkedPart<RecipeItemViewModel>? LinkedSelectedRecipe { get => _selectedRecipe; private set => SetProperty(ref _selectedRecipe, value); }
            

        // Info updating
        protected override Dictionary<string, Action<RecipeComponentDto>> ConfigureUpdaters() => new()
        {
            { nameof(RecipeComponentDto.Quantity), dto => Quantity = dto.Quantity },
            { nameof(RecipeComponentDto.SelectedRecipeUid), 
                dto => LinkedSelectedRecipe = 
                dto.SelectedRecipeUid is Guid sRecipeUid 
                ? _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(sRecipeUid) 
                : null},
            { nameof(RecipeComponentDto.ResourceUid),
                dto => LinkedResource =
                dto.ResourceUid is Guid resourceUid
                ? _linkedPartsManager.CreateAndRegisterLinkedResourceVM(resourceUid)
                : null},
        };

        private void OnRecipeComponentUpdated(RecipeComponentUpdatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.Uid) return;

            Update(ev.RecipeComponent, ev.ChangedProperties);
        }

        public void Dispose()
        {
            _updatedSubscription.Dispose();

            _store.RemoveRecipeComponent(Uid);
        }

        // Commands
        [RelayCommand]
        public async Task SetQuantityAsync(double value)
        {
            var grandParentUid = LinkedParentRecipe!.Value!.LinkedParentResource!.Uid;
            var uid = Uid;
            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentQuantityCommand>(grandParentUid, uid, value);
        }

        // For UI
        public RecipeComponentUIState Ui => _uiStateService.GetOrCreate(this);
    }
}
