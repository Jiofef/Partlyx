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
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IRecipeComponentItemUiStateService _uiStateService;
        private readonly ICommandServices _commands;

        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _parentRemoveSubscription;

        public RecipeComponentItemViewModel(RecipeComponentDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus, IRecipeComponentItemUiStateService uiStateS, ICommandServices cs)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;
            _commands = cs;

            // Info
            _parentRecipeUid = dto.ParentRecipeUid;
            ResourceUid = dto.ResourceUid;
            _quantity = dto.Quantity;
            _selectedRecipeUid = dto.SelectedRecipeUid;
            _parentRecipe = _selectedRecipeUid != null ? _store.Recipes.GetValueOrDefault((Guid)_selectedRecipeUid) : null;

            // Info updating binding
            _updatedSubscription = _bus.Subscribe<RecipeComponentUpdatedEvent>(OnRecipeComponentUpdated, true);
            _childAddSubscription = bus.Subscribe<ResourceCreatedEvent>(OnResourceCreated, true);
            _childRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(OnResourceDeleted, true);
        }

        // Recipe component info
        public Guid Uid { get; }

        private Guid? _parentRecipeUid;
        public Guid? ParentRecipeUid
        {
            get => _parentRecipeUid;
            set
            {
                SetProperty(ref _parentRecipeUid, value);
                ParentRecipe = value != null ? _store.Recipes.GetValueOrDefault((Guid)value) : null;
            }
        }

        private RecipeItemViewModel? _parentRecipe;
        // It's better to make the setter here private in future 
        public RecipeItemViewModel? ParentRecipe { get => _parentRecipe; set => SetProperty(ref _parentRecipe, value); }
        private void UpdateParentRecipe() =>
            ParentRecipe = _parentRecipeUid != null ? _store.Recipes.GetValueOrDefault((Guid)_parentRecipeUid) : null;

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

        // Commands
        [RelayCommand]
        public async Task SetQuantityAsync(double value)
        {
            var grandParentUid = ParentRecipe!.ParentResourceUid!;
            var uid = Uid;
            await _commands.CreateAsyncEndExcecuteAsync<SetRecipeComponentQuantityCommand>(grandParentUid, uid, value);
        }

        // For UI
        public RecipeComponentUIState Ui => _uiStateService.GetOrCreate(this);
    }
}
