using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class RecipeItemViewModel : UpdatableViewModel<RecipeDto>, IDisposable
    {
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;

        private readonly IEventBus _bus;
        private readonly IDisposable _subscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;

        public RecipeItemViewModel(RecipeDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;

            // Info
            _craftAmount = dto.CraftAmount;

            foreach (var component in dto.Components)
            {
                var vm = _partsFactory.CreateRecipeComponentVM(component);
                _components.Add(vm);
            }

            // Info updating binding
            _subscription = _bus.Subscribe<RecipeUpdatedEvent>(OnRecipeUpdated, true);
            _childAddSubscription = bus.Subscribe<RecipeComponentCreatedEvent>(OnComponentCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeComponentDeletedEvent>(OnComponentDeleted, true);
        }

        // Recipe info
        public Guid Uid { get; }

        private double _craftAmount;
        public double CraftAmount { get => _craftAmount; set => SetProperty(ref _craftAmount, value); }

        private ObservableCollection<RecipeComponentItemViewModel> _components = new();
        public ObservableCollection<RecipeComponentItemViewModel> Components { get => _components; } // Updates locally when component is created/removed

        // Info updating
        protected override Dictionary<string, Action<RecipeDto>> ConfigureUpdaters() => new()
        {
            
        };

        private void OnRecipeUpdated(RecipeUpdatedEvent ev)
        {
            if (Uid != ev.Recipe.Uid) return;

            Update(ev.Recipe, ev.ChangedProperties);
        }

        private void OnComponentCreated(RecipeComponentCreatedEvent ev)
        {
            if (Uid != ev.RecipeComponent.ParentRecipeUid) return;

            var componentVM = _partsFactory.CreateRecipeComponentVM(ev.RecipeComponent);
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

        public void Dispose()
        {
            _subscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();

            foreach(var component in Components)
                component.Dispose();

            _store.Recipes.Remove(Uid);
        }
    }
}
