using Partlyx.Core.Partlyx;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using Partlyx.ViewModels.PartsViewModels;
using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeViewModel : UpdatableViewModel<RecipeDto>, IVMPart, IObservableFindableIconHolder, IInheritingIconHolderViewModel, ITypedVMPartHolder<RecipeViewModel>
    {
        // Services
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeItemUiStateService _uiStateService;
        private readonly ILinkedPartsManager _linkedPartsManager;
        private readonly IconServiceViewModel _iconService;
        private readonly IEventBus _bus;
        public PartsServiceViewModel Services { get; }

        // Aggregators
        private readonly ResourceQuantityAggregator _quantityAggregator;
        private readonly ResourceLinksManager _linksManager;

        public RecipeViewModel(RecipeDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav, IVMPartsStore store,
            IVMPartsFactory partsFactory, IRecipeItemUiStateService uiStateS, ILinkedPartsManager lpm, IconServiceViewModel iconService, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _store = store;
            _partsFactory = partsFactory;
            _uiStateService = uiStateS;
            _linkedPartsManager = lpm;
            _iconService = iconService;
            _bus = bus;

            // Info
            _name = dto.Name;
            _isReversible = dto.IsReversible;
            InputsDic = new(_inputsDic);
            OutputsDic = new(_outputsDic);

            foreach (var component in dto.Inputs)
            {
                var vm = _partsFactory.GetOrCreateRecipeComponentVM(component);
                _inputs.Add(vm);
                _inputsDic.Add(vm.Uid, vm);
            }

            foreach (var component in dto.Outputs)
            {
                var vm = _partsFactory.GetOrCreateRecipeComponentVM(component);
                _outputs.Add(vm);
                _outputsDic.Add(vm.Uid, vm);
            }

            // Initialize DisplayChildren
            var inputsGroupHeader = new RecipeComponentInputGroup("Inputs", Inputs, this);
            var outputsGroupHeader = new RecipeComponentOutputGroup("Outputs", Outputs, this);
            _componentGroups = new() { inputsGroupHeader, outputsGroupHeader };
            ComponentGroups = new(_componentGroups);

            // Initialize aggregators
            _quantityAggregator = new ResourceQuantityAggregator(this);
            _quantityAggregator.InitializeFromComponents(_inputs, _outputs);

            _linksManager = new ResourceLinksManager(this, _bus);
            _linksManager.InitializeFromComponents(_inputs, _outputs);
            _linksManager.PublishAllResourceLinkChanges();

            // For the most part, we don't really care when the icon will be loaded. Until then, the icon will be empty.
            _icon = new IconViewModel();
            _ = UpdateIconFromDtoAsync(dto.Icon);
        }
        private async Task UpdateIconFromDtoAsync(IconDto dto)
        {
            Icon?.Dispose();
            Icon = await _iconService.CreateFromDtoAsync(dto);
        }

        // Recipe info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Recipe; }

        public Guid Uid { get; }

        private GuidLinkedPart<ResourceViewModel>? _parentResource;
        public GuidLinkedPart<ResourceViewModel>? LinkedParentResource { get => _parentResource; set => SetProperty(ref _parentResource, value); }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private bool _isReversible;
        public bool IsReversible
        {
            get => _isReversible;
            set
            {
                if (SetProperty(ref _isReversible, value))
                {
                    PublishAllResourceLinkChanges();
                }
            }
        }

        private ObservableCollection<RecipeComponentViewModel> _inputs = new();
        public ObservableCollection<RecipeComponentViewModel> Inputs { get => _inputs; }

        private readonly Dictionary<Guid, RecipeComponentViewModel> _inputsDic = new();
        public ReadOnlyDictionary<Guid, RecipeComponentViewModel> InputsDic { get; }

        private ObservableCollection<RecipeComponentViewModel> _outputs = new();
        public ObservableCollection<RecipeComponentViewModel> Outputs { get => _outputs; }

        private readonly Dictionary<Guid, RecipeComponentViewModel> _outputsDic = new();
        public ReadOnlyDictionary<Guid, RecipeComponentViewModel> OutputsDic { get; }

        public ObservableCollection<RecipeComponentViewModel> GetComponents(bool isOutput)
            => isOutput ? Outputs : Inputs;

        // Resource quantity sums - delegated to aggregator
        public ReadOnlyDictionary<Guid, double> InputResourceQuantities => _quantityAggregator.InputQuantities;
        public ReadOnlyDictionary<Guid, double> OutputResourceQuantities => _quantityAggregator.OutputQuantities;

        // Resource links - delegated to manager
        public HashSet<Guid> InputResources => _linksManager.InputResources;
        public HashSet<Guid> OutputResources => _linksManager.OutputResources;

        public RecipeComponentViewModel? GetChildOrNull(Guid uid)
            => _inputsDic.GetValueOrDefault(uid) ?? _outputsDic.GetValueOrDefault(uid);

        /// <summary>
        /// Checks if the resource is present in the recipe's inputs or outputs
        /// </summary>
        public bool HasResource(Guid resourceUid) 
            => HasResourceInInputs(resourceUid) || HasResourceInOutputs(resourceUid);

        /// <summary>
        /// Checks if the resource is present in the recipe's inputs
        /// </summary>
        public bool HasResourceInInputs(Guid resourceUid) 
            => _linksManager.HasResourceInInputs(resourceUid);

        /// <summary>
        /// Checks if the resource is present in the recipe's outputs
        /// </summary>
        public bool HasResourceInOutputs(Guid resourceUid) 
            => _linksManager.HasResourceInOutputs(resourceUid);

        /// <summary>
        /// Checks if the resource is present in the recipe's components that can be used as default recipe
        /// If recipe is reversible, checks both inputs and outputs; otherwise, only outputs
        /// </summary>
        public bool HasResourceInLinkedComponents(Guid resourceUid)
            => IsReversible ? HasResource(resourceUid) : HasResourceInOutputs(resourceUid);

        private void AddComponent(RecipeComponentViewModel component)
        {
            if (component.IsOutput)
            {
                if (_outputsDic.ContainsKey(component.Uid))
                    return;

                Outputs.Add(component);
                _outputsDic.Add(component.Uid, component);
                _linksManager.AddComponent(RecipeComponentType.Output, component.LinkedResource?.Uid ?? Guid.Empty);
                _quantityAggregator.AddComponent(component);
            }
            else
            {
                if (_inputsDic.ContainsKey(component.Uid))
                    return;

                Inputs.Add(component);
                _inputsDic.Add(component.Uid, component);
                _linksManager.AddComponent(RecipeComponentType.Input, component.LinkedResource?.Uid ?? Guid.Empty);
                _quantityAggregator.AddComponent(component);
            }
        }

        private void RemoveComponent(RecipeComponentViewModel component)
        {
            if (_inputsDic.ContainsKey(component.Uid))
            {
                Inputs.Remove(component);
                _inputsDic.Remove(component.Uid);
                _linksManager.RemoveComponent(RecipeComponentType.Input, component.LinkedResource?.Uid ?? Guid.Empty);
                _quantityAggregator.RemoveComponent(component);
            }
            else if (_outputsDic.ContainsKey(component.Uid))
            {
                Outputs.Remove(component);
                _outputsDic.Remove(component.Uid);
                _linksManager.RemoveComponent(RecipeComponentType.Output, component.LinkedResource?.Uid ?? Guid.Empty);
                _quantityAggregator.RemoveComponent(component);
            }
        }

        private void PublishAllResourceLinkChanges()
        {
            _linksManager.PublishAllResourceLinkChanges();
        }

        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; private set => SetProperty(ref _icon, value); }
        public InheritedIcon.InheritedIconParentTypeEnum InheritedIconParentDefaultType => InheritedIcon.InheritedIconParentTypeEnum.Resource;
        public Guid? InheritedIconDefaultParentUid => LinkedParentResource?.Uid;
        public bool CanContainInheritedIcon => true;

        // Info updating
        protected override Dictionary<string, Action<RecipeDto>> ConfigureUpdaters() => new()
        {
            { nameof(RecipeDto.Name), dto => Name = dto.Name },
            { nameof(RecipeDto.IsReversible), dto => IsReversible = dto.IsReversible },
            { nameof(RecipeDto.Icon), dto => _ = UpdateIconFromDtoAsync(dto.Icon) },
        };

        public void HandleEvent(object @event)
        {
            switch (@event)
            {
                case RecipeUpdatedEvent rue:
                    OnRecipeUpdated(rue);
                    break;
                case RecipeComponentCreatedEvent rcce:
                    OnComponentCreated(rcce);
                    break;
                case RecipeComponentDeletingStartedEvent rcde:
                    OnComponentDeletingStarted(rcde);
                    break;
                case RecipeComponentMovedEvent rcme:
                    OnComponentMoved(rcme);
                    break;
                case RecipeComponentUpdatedEvent rcue:
                    OnComponentUpdated(rcue);
                    break;
                case RecipeComponentQuantityChangedEvent rcqc:
                    OnComponentQuantityChanged(rcqc);
                    break;
            }
        }

        private void OnComponentQuantityChanged(RecipeComponentQuantityChangedEvent ev)
        {
            // Only process events for components that belong to this recipe
            if (GetChildOrNull(ev.ComponentUid) == null) return;

            _quantityAggregator.UpdateComponentQuantity(ev.ComponentType, ev.ResourceUid, ev.OldQuantity, ev.NewQuantity);
        }
        private void OnRecipeUpdated(RecipeUpdatedEvent ev)
        {
            if (Uid != ev.Recipe.Uid) return;

            Update(ev.Recipe, ev.ChangedProperties);
        }

        private void OnComponentCreated(RecipeComponentCreatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.ParentRecipeUid) return;

            var componentVM = _partsFactory.GetOrCreateRecipeComponentVM(ev.RecipeComponent);
            AddComponent(componentVM);

            UiItem.IsExpanded = true;
        }

        private void OnComponentDeletingStarted(RecipeComponentDeletingStartedEvent ev)
        {
            if (Uid != ev.ParentRecipeUid) return;

            var componentVM = GetChildOrNull(ev.ComponentUid);
            if (componentVM != null)
            {
                RemoveComponent(componentVM);
                componentVM.Dispose();
            }
        }

        private void OnComponentMoved(RecipeComponentMovedEvent ev)
        {
            if (Uid == ev.OldRecipeUid)
            {
                var componentVM = GetChildOrNull(ev.ComponentUid);
                if (componentVM != null)
                    RemoveComponent(componentVM);
            }
            else if (Uid == ev.NewRecipeUid)
            {
                var componentVM = _store.Components.GetValueOrDefault(ev.ComponentUid);
                if (componentVM != null)
                {
                    AddComponent(componentVM);
                    componentVM.LinkedParentRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(Uid);
                }
            }
        }

        private void OnComponentUpdated(RecipeComponentUpdatedEvent ev)
        {
            if (ev.ChangedProperties?.TryGetValue("ResourceUid", out var pair) == true)
            {
                var componentVM = GetChildOrNull(ev.RecipeComponent.Uid);
                if (componentVM != null)
                {
                    var oldResourceUid = (Guid)pair.OldValue;
                    var newResourceUid = (Guid)pair.NewValue;

                    if (oldResourceUid != newResourceUid)
                    {
                        _linksManager.RemoveComponent(componentVM.ComponentType, oldResourceUid);
                        _linksManager.AddComponent(componentVM.ComponentType, newResourceUid);
                    }
                }
            }
        }

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeComponentViewModel component) => AddComponent(component);

        public void Dispose()
        {
            UiItem.Dispose();

            // We call ToList to avoid an error due to collection changes during the loop.
            foreach (var component in Inputs.ToList())
            {
                component.Dispose();
                RemoveComponent(component);
                _store.RemoveRecipeComponent(component.Uid);
            }
            foreach (var component in Outputs.ToList())
            {
                component.Dispose();
                RemoveComponent(component);
                _store.RemoveRecipeComponent(component.Uid);
            }
        }

        public override void Update(RecipeDto dto, IReadOnlyList<string>? changedProperties = null)
        {
            base.Update(dto, changedProperties);

            var eventRecieversKeys = new HashSet<object>();
            if (changedProperties != null)
                foreach (var property in changedProperties)
                    eventRecieversKeys.Add(property);
            eventRecieversKeys.Add(dto.Uid);
            _bus.Publish(new RecipeUpdatedViewModelEvent(Uid, changedProperties, eventRecieversKeys));
        }

        // For UI
        private readonly ObservableCollection<RecipeComponentGroup> _componentGroups;
        public ReadOnlyObservableCollection<RecipeComponentGroup> ComponentGroups { get; }

        public RecipeItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        FocusableItemUIState IFocusable.UiItem => UiItem;
        public RecipeNodeUIState UiNode => _uiStateService.GetOrCreateNodeUi(this);
        public PartsGlobalNavigations GlobalNavigations { get; }

        // Compatibility
        /// <summary> Self </summary>
        public RecipeViewModel Part => this;
    }
}
