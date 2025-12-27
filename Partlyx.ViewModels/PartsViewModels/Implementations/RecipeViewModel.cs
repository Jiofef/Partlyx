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
        public bool IsReversible { get => _isReversible; set => SetProperty(ref _isReversible, value); }

        private ObservableCollection<RecipeComponentViewModel> _inputs = new();
        public ObservableCollection<RecipeComponentViewModel> Inputs { get => _inputs; }

        private readonly Dictionary<Guid, RecipeComponentViewModel> _inputsDic = new();
        public ReadOnlyDictionary<Guid, RecipeComponentViewModel> InputsDic { get; }

        private ObservableCollection<RecipeComponentViewModel> _outputs = new();
        public ObservableCollection<RecipeComponentViewModel> Outputs { get => _outputs; }

        private readonly Dictionary<Guid, RecipeComponentViewModel> _outputsDic = new();
        public ReadOnlyDictionary<Guid, RecipeComponentViewModel> OutputsDic { get; }

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
            return _inputs.Any(c => c.LinkedResource?.Uid == resourceUid);
        }

        /// <summary>
        /// Checks if the resource is present in the recipe's outputs
        /// </summary>
        public bool HasResourceInOutputs(Guid resourceUid)
        {
            return _outputs.Any(c => c.LinkedResource?.Uid == resourceUid);
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
            }
            else
            {
                if (_inputsDic.ContainsKey(component.Uid))
                    return;

                Inputs.Add(component);
                _inputsDic.Add(component.Uid, component);
            }
        }

        private void RemoveComponent(RecipeComponentViewModel component)
        {
            if (_inputsDic.ContainsKey(component.Uid))
            {
                Inputs.Remove(component);
                _inputsDic.Remove(component.Uid);
            }
            else if (_outputsDic.ContainsKey(component.Uid))
            {
                Outputs.Remove(component);
                _outputsDic.Remove(component.Uid);
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

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeComponentViewModel component)
        {
            AddComponent(component);
        }

        public void Dispose()
        {
            UiItem.Dispose();

            foreach(var component in Inputs)
                component.Dispose();
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
        public RecipeItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        PartItemUIState IVMPart.UiItem => UiItem;
        public RecipeNodeUIState UiNode => _uiStateService.GetOrCreateNodeUi(this);
        public PartsGlobalNavigations GlobalNavigations { get; }

        // Compatibility
        /// <summary> Self </summary>
        public RecipeViewModel? Part => this;
    }
}
