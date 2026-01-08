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

            // Initialize resource counts
            foreach (var component in _inputs)
            {
                UpdateResourceCount(RecipeComponentType.Input, component.LinkedResource?.Uid ?? Guid.Empty, 1);
            }
            foreach (var component in _outputs)
            {
                UpdateResourceCount(RecipeComponentType.Output, component.LinkedResource?.Uid ?? Guid.Empty, 1);
            }

            // For the most part, we don't really care when the icon will be loaded. Until then, the icon will be empty.
            _icon = new IconViewModel();
            _ = UpdateIconFromDto(dto.Icon);
        }
        private async Task UpdateIconFromDto(IconDto dto)
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

        // Optimized resource counting
        private readonly Dictionary<Guid, int> _inputsResourceCounts = new();
        public HashSet<Guid> InputResources { get; } = new();
        private readonly Dictionary<Guid, int> _outputsResourceCounts = new();
        public HashSet<Guid> OutputResources { get; } = new();

        public RecipeComponentViewModel? GetChildOrNull(Guid uid)
        {
            return _inputsDic.GetValueOrDefault(uid) ?? _outputsDic.GetValueOrDefault(uid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's inputs or outputs
        /// </summary>
        public bool HasResource(Guid resourceUid)
        {
            return HasResourceInInputs(resourceUid) || HasResourceInOutputs(resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's inputs
        /// </summary>
        public bool HasResourceInInputs(Guid resourceUid)
        {
            return _inputsResourceCounts.ContainsKey(resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's outputs
        /// </summary>
        public bool HasResourceInOutputs(Guid resourceUid)
        {
            return _outputsResourceCounts.ContainsKey(resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's components that can be used as default recipe
        /// If recipe is reversible, checks both inputs and outputs; otherwise, only outputs
        /// </summary>
        public bool HasResourceInLinkedComponents(Guid resourceUid)
        {
            if (IsReversible)
                return HasResource(resourceUid);
            else
                return HasResourceInOutputs(resourceUid);
        }

        private void AddComponent(RecipeComponentViewModel component)
        {
            if (component.IsOutput)
            {
                if (_outputsDic.ContainsKey(component.Uid))
                    return;

                Outputs.Add(component);
                _outputsDic.Add(component.Uid, component);
                UpdateResourceCount(RecipeComponentType.Output, component.LinkedResource?.Uid ?? Guid.Empty, 1);
            }
            else
            {
                if (_inputsDic.ContainsKey(component.Uid))
                    return;

                Inputs.Add(component);
                _inputsDic.Add(component.Uid, component);
                UpdateResourceCount(RecipeComponentType.Input, component.LinkedResource?.Uid ?? Guid.Empty, 1);
            }
        }

        private void RemoveComponent(RecipeComponentViewModel component)
        {
            if (_inputsDic.ContainsKey(component.Uid))
            {
                Inputs.Remove(component);
                _inputsDic.Remove(component.Uid);
                UpdateResourceCount(RecipeComponentType.Input, component.LinkedResource?.Uid ?? Guid.Empty, -1);
            }
            else if (_outputsDic.ContainsKey(component.Uid))
            {
                Outputs.Remove(component);
                _outputsDic.Remove(component.Uid);
                UpdateResourceCount(RecipeComponentType.Output, component.LinkedResource?.Uid ?? Guid.Empty, -1);
            }
        }

        private void UpdateResourceCount(RecipeComponentType componentType, Guid resourceUid, int delta)
        {
            if (resourceUid == Guid.Empty) return;

            Dictionary<Guid, int> dict = componentType == RecipeComponentType.Input ? _inputsResourceCounts : _outputsResourceCounts;
            HashSet<Guid> hashes = componentType == RecipeComponentType.Input ? InputResources : OutputResources;

            bool wasPresent = dict.ContainsKey(resourceUid);
            if (dict.TryGetValue(resourceUid, out int count))
            {
                count += delta;
                if (count > 0)
                {
                    dict[resourceUid] = count;
                }
                else
                {
                    dict.Remove(resourceUid);
                    hashes?.Remove(resourceUid);
                }
            }
            else if (delta > 0)
            {
                dict[resourceUid] = delta;
                hashes?.Add(resourceUid);
            }

            // Publish event if the presence changed
            if (wasPresent != dict.ContainsKey(resourceUid))
            {
                PublishResourceLinkChanged(resourceUid);
            }
        }

        private RecipeResourceLinkTypeEnum GetLinkTypeForResource(Guid resourceUid)
        {
            bool inInputs = InputResources.Contains(resourceUid);
            bool inOutputs = OutputResources.Contains(resourceUid);

            if (IsReversible)
            {
                if (inInputs && inOutputs) return RecipeResourceLinkTypeEnum.Both;
                if (inInputs) return RecipeResourceLinkTypeEnum.Receiving;
                if (inOutputs) return RecipeResourceLinkTypeEnum.Producing;
                return RecipeResourceLinkTypeEnum.None;
            }
            else
            {
                return inOutputs ? RecipeResourceLinkTypeEnum.Producing : RecipeResourceLinkTypeEnum.None;
            }
        }

        private void PublishResourceLinkChanged(Guid resourceUid)
        {
            var linkType = GetLinkTypeForResource(resourceUid);
            _bus.Publish(new RecipeResourceLinkChangedEvent(this, resourceUid, linkType));
        }

        private void PublishAllResourceLinkChanges()
        {
            var allResources = new HashSet<Guid>(InputResources);
            allResources.UnionWith(OutputResources);

            foreach (var resourceUid in allResources)
            {
                PublishResourceLinkChanged(resourceUid);
            }
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
            { nameof(RecipeDto.Icon), dto => _ = UpdateIconFromDto(dto.Icon) },
        };

        public void HandleEvent(object @event)
        {
            if (@event is RecipeUpdatedEvent rue)
            {
                OnRecipeUpdated(rue);
                return;
            }
            if (@event is RecipeComponentCreatedEvent rcce)
            {
                OnComponentCreated(rcce);
                return;
            }
            if (@event is RecipeComponentDeletingStartedEvent rcde)
            {
                OnComponentDeletingStarted(rcde);
                return;
            }
            if (@event is RecipeComponentMovedEvent rcme)
            {
                OnComponentMoved(rcme);
                return;
            }
            if (@event is RecipeComponentUpdatedEvent rcue)
            {
                OnComponentUpdated(rcue);
                return;
            }
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
                {
                    RemoveComponent(componentVM);
                }
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
                        UpdateResourceCount(componentVM.ComponentType, oldResourceUid, -1);
                        UpdateResourceCount(componentVM.ComponentType, newResourceUid, 1);
                    }
                }
            }
        }

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeComponentViewModel component)
        {
            AddComponent(component);
        }

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
