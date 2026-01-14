using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using Partlyx.ViewModels.PartsViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeComponentViewModel : UpdatableViewModel<RecipeComponentDto>, IVMPart, ITypedVMPartHolder<RecipeComponentViewModel>
    {
        // Servicess
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeComponentItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;
        private readonly ILinkedPartsManager _linkedPartsManager;
        private readonly IEventBus _bus;
        public PartsServiceViewModel Services { get; }

        // Events
        private readonly IDisposable _currentRecipeDefaultRecipeUpdateSubscription;
        private readonly IDisposable _currentRecipeDefaultRecipeValidSubscription;
        private readonly IDisposable _currentRecipeSelectedRecipeUpdateSubscription;
        private readonly IDisposable _iconUpdatedSubscription;

        public RecipeComponentViewModel(RecipeComponentDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav, IVMPartsStore store,
            IVMPartsFactory partsFactory, IRecipeComponentItemUiStateService uiStateS, ICommandServices cs, ILinkedPartsManager lpm, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _store = store;
            _partsFactory = partsFactory;
            _uiStateService = uiStateS;
            _commands = cs;
            _linkedPartsManager = lpm;
            _bus = bus;

            // Info
            if (dto.ParentRecipeUid is Guid parentUid)
                LinkedParentRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(parentUid);
            LinkedResource = _linkedPartsManager.CreateAndRegisterLinkedResourceVM(dto.ResourceUid);

            // SelectedRecipeComponents is a helper property for fast switching from one component to its child components in the tree.
            // His calculation requires two property chains to be taken into account, and this is carried out in two compact subscriptions
            _currentRecipeDefaultRecipeUpdateSubscription = this
                .WhenAnyValue(x => x.LinkedResource!.Value!.LinkedDefaultRecipe!.Value)
                .Subscribe(v => UpdateCurrentRecipe());

            _currentRecipeDefaultRecipeValidSubscription = this
                .WhenAnyValue(x => x.LinkedResource!.Value!.IsDefaultRecipeValid)
                .Subscribe(v => UpdateCurrentRecipe());

            _currentRecipeSelectedRecipeUpdateSubscription = this
                .WhenAnyValue(x => x.LinkedSelectedRecipe!.Value)
                .Subscribe(v => UpdateCurrentRecipe());
            UpdateCurrentRecipe();

            _iconUpdatedSubscription = this
                .WhenAnyValue(x => x.LinkedResource!.Value!.Icon)
                .Subscribe(r => OnPropertyChanged(nameof(Icon)));


            _quantity = dto.Quantity;
            UiItem.VisualQuantity = dto.Quantity;
            _isOutput = dto.IsOutput;
            if (dto.SelectedRecipeUid is Guid selectedUid)
                LinkedSelectedRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(selectedUid);
        }

        // Recipe component info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Component; }

        public Guid Uid { get; }

        private GuidLinkedPart<RecipeViewModel>? _parentRecipe;
        public GuidLinkedPart<RecipeViewModel>? LinkedParentRecipe { get => _parentRecipe; set => SetProperty(ref _parentRecipe, value); }
        public RecipeViewModel ParentRecipe => LinkedParentRecipe?.Value!;

        private GuidLinkedPart<ResourceViewModel>? _resource;
        public GuidLinkedPart<ResourceViewModel>? LinkedResource { get => _resource; private set => SetProperty(ref _resource, value); }
        public ResourceViewModel? Resource => LinkedResource?.Value;

        private double _quantity;
        public double Quantity
        {
            get => _quantity;
            set
            {
                double oldValue = _quantity;
                if (SetProperty(ref _quantity, value))
                {
                    UiItem.VisualQuantity = value;
                    PublishQuantityChangedEvent(oldValue, value);
                }
            }
        }

        private void PublishQuantityChangedEvent(double oldValue, double newValue)
        {
            var resourceUid = LinkedResource?.Uid ?? Guid.Empty;
            _bus.Publish(new RecipeComponentQuantityChangedEvent(
                Uid,
                resourceUid,
                ComponentType,
                oldValue,
                newValue,
                Uid));
        }

        private bool _isOutput;
        public bool IsOutput { get => _isOutput;
            set
            {
                if (SetProperty(ref _isOutput, value))
                    OnPropertyChanged(nameof(ComponentType));
            }
        }
        public RecipeComponentType ComponentType => IsOutput ? RecipeComponentType.Output : RecipeComponentType.Input;

        private GuidLinkedPart<RecipeViewModel>? _selectedRecipe;
        public GuidLinkedPart<RecipeViewModel>? LinkedSelectedRecipe { get => _selectedRecipe; private set => SetProperty(ref _selectedRecipe, value); }

        private RecipeViewModel? _currentRecipe;

        public RecipeViewModel? CurrentRecipe { get => _currentRecipe; private set => SetProperty(ref _currentRecipe, value); }

        public IconViewModel Icon { get => LinkedResource?.Value?.Icon!; }


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
            { nameof(RecipeComponentDto.IsOutput), dto => IsOutput = dto.IsOutput }
        };

        public void HandleEvent(object @event)
        {
            if (@event is RecipeComponentUpdatedEvent rcue)
            {
                OnRecipeComponentUpdated(rcue);
                return;
            }
        }
        private void OnRecipeComponentUpdated(RecipeComponentUpdatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.Uid) return;

            Update(ev.RecipeComponent, ev.ChangedProperties?.Keys.ToList());
        }

        public void Dispose()
        {
            UiItem.Dispose();

            _currentRecipeDefaultRecipeUpdateSubscription.Dispose();
            _currentRecipeSelectedRecipeUpdateSubscription.Dispose();
            _iconUpdatedSubscription.Dispose();
        }

        public override void Update(RecipeComponentDto dto, IReadOnlyList<string>? changedProperties = null)
        {
            base.Update(dto, changedProperties);

            var eventRecieversKeys = new HashSet<object>();
            if (changedProperties != null)
                foreach (var property in changedProperties)
                    eventRecieversKeys.Add(property);
            eventRecieversKeys.Add(dto.Uid);
            _bus.Publish(new RecipeComponentUpdatedViewModelEvent(Uid, changedProperties, eventRecieversKeys));
        }

        // For UI
        public RecipeComponentItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        FocusableItemUIState IFocusable.UiItem => UiItem;
        public RecipeComponentNodeUIState UiNode => _uiStateService.GetOrCreateNodeUi(this);
        public PartsGlobalNavigations GlobalNavigations { get; }
        private void UpdateCurrentRecipe()
        {
            bool hasValidSelectedRecipe = LinkedSelectedRecipe?.Value != null;
            bool hasValidDefaultRecipe = LinkedResource?.Value?.LinkedDefaultRecipe?.Value != null && LinkedResource.Value.IsDefaultRecipeValid;

            if (hasValidSelectedRecipe)
                CurrentRecipe = LinkedSelectedRecipe?.Value;
            else if (hasValidDefaultRecipe)
                CurrentRecipe = LinkedResource?.Value?.LinkedDefaultRecipe?.Value;
            else
                CurrentRecipe = null;
        }

        // Compatibility
        /// <summary> Self </summary>
        public RecipeComponentViewModel Part => this;
    }
}
