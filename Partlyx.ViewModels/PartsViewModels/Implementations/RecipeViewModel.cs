using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using Partlyx.ViewModels.UIStates;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class RecipeViewModel : UpdatableViewModel<RecipeDto>, IVMPart
    {
        // Services
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeItemUiStateService _uiStateService;
        private readonly ILinkedPartsManager _linkedPartsManager;
        public PartsServiceViewModel Services { get; }

        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _childMoveSubscription;

        public RecipeViewModel(RecipeDto dto, PartsServiceViewModel service, PartsGlobalNavigations nav, IVMPartsStore store,
            IVMPartsFactory partsFactory, IEventBus bus, IRecipeItemUiStateService uiStateS, ILinkedPartsManager lpm)
        {
            Uid = dto.Uid;

            // Services
            Services = service;
            GlobalNavigations = nav;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;
            _linkedPartsManager = lpm;

            // Info
            if (dto.ParentResourceUid is Guid parentUid)
            {
                var parentLinked = _linkedPartsManager.CreateAndRegisterLinkedResourceVM(parentUid);
                _parentResource = parentLinked;
            }

            _name = dto.Name;
            _craftAmount = dto.CraftAmount;
            
            foreach (var component in dto.Components)
            {
                var vm = _partsFactory.GetOrCreateRecipeComponentVM(component);
                _components.Add(vm);
            }

            _icon = new IconViewModel(dto.Icon);

            // Info updating binding
            _updatedSubscription = bus.Subscribe<RecipeUpdatedEvent>(OnRecipeUpdated, true);
            _childAddSubscription = bus.Subscribe<RecipeComponentCreatedEvent>(OnComponentCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeComponentDeletedEvent>(OnComponentDeleted, true);
            _childMoveSubscription = bus.Subscribe<RecipeComponentMovedEvent>(OnComponentMoved, true);
        }

        // Recipe info
        public PartTypeEnumVM PartType { get => PartTypeEnumVM.Recipe; }

        public Guid Uid { get; }

        private GuidLinkedPart<ResourceViewModel>? _parentResource;
        public GuidLinkedPart<ResourceViewModel>? LinkedParentResource { get => _parentResource; set => SetProperty(ref _parentResource, value); }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private double _craftAmount;
        public double CraftAmount { get => _craftAmount; set => SetProperty(ref _craftAmount, value); }

        private ObservableCollection<RecipeComponentViewModel> _components = new();

        public ObservableCollection<RecipeComponentViewModel> Components { get => _components; } // Updates locally when component is created/removed

        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; private set => SetProperty(ref _icon, value); }

        // Info updating
        protected override Dictionary<string, Action<RecipeDto>> ConfigureUpdaters() => new()
        {
            { nameof(RecipeDto.Name), dto => Name = dto.Name },
            { nameof(RecipeDto.CraftAmount), dto => CraftAmount = dto.CraftAmount },
            { nameof(RecipeDto.Icon), dto => Icon.UpdateFromDto(dto.Icon) },
        };

        private void OnRecipeUpdated(RecipeUpdatedEvent ev)
        {
            if (Uid != ev.Recipe.Uid) return;

            Update(ev.Recipe, ev.ChangedProperties);
        }

        private void OnComponentCreated(RecipeComponentCreatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.ParentRecipeUid) return;

            var componentVM = _partsFactory.GetOrCreateRecipeComponentVM(ev.RecipeComponent);
            Components.Add(componentVM);
        }

        private void OnComponentDeleted(RecipeComponentDeletedEvent ev)
        {
            if (Uid != ev.ParentRecipeUid) return;

            var componentVM = Components.FirstOrDefault(c => c.Uid == ev.RecipeComponentUid);
            if (componentVM != null)
            {
                Components.Remove(componentVM);
                componentVM.Dispose();
            }
        }

        private void OnComponentMoved(RecipeComponentMovedEvent ev)
        {
            if (Uid == ev.OldRecipeUid)
            {
                var componentVM = Components.FirstOrDefault(c => c.Uid == ev.ComponentUid);
                if (componentVM != null)
                {
                    Components.Remove(componentVM);
                }
            }
            else if (Uid == ev.NewRecipeUid)
            {
                var componentVM = _store.RecipeComponents.GetValueOrDefault(ev.ComponentUid);
                if (componentVM != null)
                {
                    Components.Add(componentVM);
                    componentVM.LinkedParentRecipe = _linkedPartsManager.CreateAndRegisterLinkedRecipeVM(Uid);
                }
            }
        }

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeComponentViewModel component)
        {
            Components.Add(component);
        }

        public void Dispose()
        {
            UiItem.Dispose();

            _updatedSubscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
            _childMoveSubscription.Dispose();

            foreach(var component in Components)
                component.Dispose();
        }

        // For UI
        public RecipeItemUIState UiItem => _uiStateService.GetOrCreateItemUi(this);
        PartItemUIState IVMPart.UiItem => UiItem;
        public RecipeNodeUIState UiNode => _uiStateService.GetOrCreateNodeUi(this);
        public PartsGlobalNavigations GlobalNavigations { get; }
    }
}
