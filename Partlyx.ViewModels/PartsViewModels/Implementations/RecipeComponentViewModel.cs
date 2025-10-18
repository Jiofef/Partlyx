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
using Partlyx.ViewModels.UIStates;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeComponentViewModel : UpdatableViewModel<RecipeComponentDto>, IVMPart
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
        private readonly IDisposable _childComponentsDefaultRecipeUpdateSubscribe;
        private readonly IDisposable _childComponentsSelectedRecipeUpdateSubscribe;

        public RecipeComponentViewModel(RecipeComponentDto dto, PartsServiceViewModel service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus, IRecipeComponentItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm)
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

            // SelectedRecipeComponents is a helper property for fast switching from one component to its child components in the tree.
            // His calculation requires two property chains to be taken into account, and this is carried out in two compact subscriptions
            _childComponentsDefaultRecipeUpdateSubscribe = this
                .WhenAnyValue(x => x.LinkedResource!.Value!.LinkedDefaultRecipe!.Value)
                .Subscribe(v => UpdateSelectedComponents());

            _childComponentsSelectedRecipeUpdateSubscribe = this
                .WhenAnyValue(x => x.LinkedSelectedRecipe!.Value)
                .Subscribe(v => UpdateSelectedComponents());
            UpdateSelectedComponents();


            _quantity = dto.Quantity;
            if (dto.SelectedRecipeUid is Guid selectedUid)
                LinkedSelectedRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(selectedUid);

            // Info updating binding
            _updatedSubscription = bus.Subscribe<RecipeComponentUpdatedEvent>(OnRecipeComponentUpdated, true);
        }

        // Recipe component info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Component; }

        public Guid Uid { get; }

        private GuidLinkedPart<RecipeViewModel>? _parentRecipe;
        public GuidLinkedPart<RecipeViewModel>? LinkedParentRecipe { get => _parentRecipe; set => SetProperty(ref _parentRecipe, value); }

        private GuidLinkedPart<ResourceViewModel>? _resource;
        public GuidLinkedPart<ResourceViewModel>? LinkedResource { get => _resource; private set => SetProperty(ref _resource, value); }

        private double _quantity;
        public double Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private GuidLinkedPart<RecipeViewModel>? _selectedRecipe;
        public GuidLinkedPart<RecipeViewModel>? LinkedSelectedRecipe { get => _selectedRecipe; private set => SetProperty(ref _selectedRecipe, value); }

        private ObservableCollection<RecipeComponentViewModel>? _selectedRecipeComponents;
        public ObservableCollection<RecipeComponentViewModel>? SelectedRecipeComponents { get => _selectedRecipeComponents; private set => SetProperty(ref _selectedRecipeComponents, value); }


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
            _childComponentsDefaultRecipeUpdateSubscribe.Dispose();
            _childComponentsSelectedRecipeUpdateSubscribe.Dispose();

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
        public RecipeComponentItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        public RecipeComponentNodeUIState UiNode => _uiStateService.GetOrCreateNodeUi(this);

        private void UpdateSelectedComponents()
        {
            SelectedRecipeComponents =
                LinkedSelectedRecipe?.Value?.Components ??
                LinkedResource?.Value?.LinkedDefaultRecipe?.Value?.Components;
        }
    }
}
