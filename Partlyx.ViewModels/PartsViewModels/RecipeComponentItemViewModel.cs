using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class RecipeComponentItemViewModel : UpdatableViewModel<RecipeComponentDto>, IDisposable
    {
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;

        private readonly IEventBus _bus;
        private readonly IDisposable _subscription;

        public RecipeComponentItemViewModel(RecipeComponentDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;

            // Info
            _quantity = dto.Quantity;

            // Info updating binding
            _subscription = _bus.Subscribe<RecipeComponentUpdatedEvent>(OnRecipeComponentUpdated, true);
        }

        // Recipe component info
        public Guid Uid { get; }
        private Guid _resourceUid;
        public Guid ResourceUid { get => _resourceUid; set => SetProperty(ref _resourceUid, value); }
        public ResourceItemViewModel? Resource => _store.Resources[ResourceUid];

        private double _quantity;
        public double Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private Guid? _parentRecipeUid;
        public Guid? ParentRecipeUid { get => _parentRecipeUid; set => SetProperty(ref _parentRecipeUid, value); }
        public RecipeItemViewModel? ParentRecipe => ParentRecipeUid != null ? _store.Recipes[(Guid)ParentRecipeUid] : null;

        private Guid? _selectedRecipeUid;
        public Guid? SelectedRecipeUid { get => _selectedRecipeUid; set => SetProperty(ref _selectedRecipeUid, value); }
        public RecipeItemViewModel? SelectedRecipe => SelectedRecipeUid != null ? _store.Recipes[(Guid)SelectedRecipeUid] : null;

        // Info updating
        protected override Dictionary<string, Action<RecipeComponentDto>> ConfigureUpdaters() => new()
        {
            
        };

        private void OnRecipeComponentUpdated(RecipeComponentUpdatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.Uid) return;

            Update(ev.RecipeComponent, ev.ChangedProperties);
        }

        public void Dispose()
        {
            _subscription.Dispose();

            _store.RecipeComponents.Remove(Uid);
        }
    }
}
