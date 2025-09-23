using Partlyx.Core;
using Partlyx.Infrastructure.Events;
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
    public class RecipeComponentItemViewModel : UpdatableViewModel<RecipeComponentDto>, IVMPart
    {
        // Servicess
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeComponentItemUiStateService _uiStateService;

        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;

        public RecipeComponentItemViewModel(RecipeComponentDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus, IRecipeComponentItemUiStateService uiStateS)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;

            // Info
            _parentRecipeUid = dto.ParentRecipeUid;
            ResourceUid = dto.ResourceUid;
            _quantity = dto.Quantity;
            _selectedRecipeUid = dto.SelectedRecipeUid;

            // Info updating binding
            _updatedSubscription = _bus.Subscribe<RecipeComponentUpdatedEvent>(OnRecipeComponentUpdated, true);
            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
        }

        // Recipe component info
        public Guid Uid { get; }

        private Guid? _parentRecipeUid;
        public Guid? ParentRecipeUid { get => _parentRecipeUid; set => SetProperty(ref _parentRecipeUid, value); }
        public RecipeItemViewModel? ParentRecipe => ParentRecipeUid != null ? _store.Recipes[(Guid)ParentRecipeUid] : null;

        private Guid _resourceUid;
        public Guid ResourceUid 
        { 
            get => _resourceUid;
            set
            {
                SetProperty(ref _resourceUid, value);
                Resource = _store.Resources.GetValueOrDefault(value);
            }
        }
        private ResourceItemViewModel? _resource;
        public ResourceItemViewModel? Resource { get => _resource; private set => SetProperty(ref _resource, value); }

        private double _quantity;
        public double Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private Guid? _selectedRecipeUid;

        public Guid? SelectedRecipeUid { get => _selectedRecipeUid; set => SetProperty(ref _selectedRecipeUid, value); }
        public RecipeItemViewModel? SelectedRecipe
        {
            get
            {
                if (SelectedRecipeUid == null || Resource == null) return null;

                var recipe = _store.Recipes[(Guid)SelectedRecipeUid];
                if (!Resource.Recipes.Contains(recipe)) return null;

                return recipe;
            }
        }
            

        // Info updating
        protected override Dictionary<string, Action<RecipeComponentDto>> ConfigureUpdaters() => new()
        {
            { nameof(RecipeComponentDto.Quantity), dto => Quantity = dto.Quantity },
            { nameof(RecipeComponentDto.SelectedRecipeUid), dto => SelectedRecipeUid = dto.SelectedRecipeUid },
        };

        private void OnRecipeComponentUpdated(RecipeComponentUpdatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.Uid) return;

            Update(ev.RecipeComponent, ev.ChangedProperties);
        }
        private void OnResourceDeleted(ResourceDeletedEvent ev)
        {
            if (ev.ResourceUid == ResourceUid)
            {
                Resource = null;
            }
        }
        private void OnResourceCreated(ResourceCreatedEvent ev)
        {
            if (ev.Resource.Uid == ResourceUid)
            {
                Resource = _partsFactory.GetOrCreateResourceVM(ev.Resource);
            }
        }

        public void Dispose()
        {
            _updatedSubscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();

            _store.RecipeComponents.Remove(Uid);
        }

        // For UI
        public RecipeComponentUIState Ui => _uiStateService.GetOrCreate(Uid);
    }
}
