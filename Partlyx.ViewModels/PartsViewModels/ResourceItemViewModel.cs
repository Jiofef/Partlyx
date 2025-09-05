using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Partlyx.ViewModels.PartsViewModels
{
    public class ResourceItemViewModel : UpdatableViewModel<ResourceDto>, IDisposable
    {
        // Services
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        
        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _subscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _childMoveSubscription;

        public ResourceItemViewModel(ResourceDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;

            // Info
            _name = dto.Name;
            _defaultRecipeUid = dto.DefaultRecipeUid;

            foreach (var recipe in dto.Recipes)
            {
                var vm = _partsFactory.CreateRecipeVM(recipe);
                _recipes.Add(vm);
            }

            // Info updating binding
            _subscription = _bus.Subscribe<ResourceUpdatedEvent>(OnResourceUpdated, true);
            _childAddSubscription = bus.Subscribe<RecipeCreatedEvent>(OnRecipeCreated, true);
            _childRemoveSubscription = bus.Subscribe<RecipeDeletedEvent>(OnRecipeDeleted, true);
            _childMoveSubscription = bus.Subscribe<RecipeMovedEvent>(OnRecipeMoved, true);
        }

        // Resource info
        public Guid Uid { get; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private ObservableCollection<RecipeItemViewModel> _recipes = new();

        public ObservableCollection<RecipeItemViewModel> Recipes { get => _recipes; } // Updates locally when recipe is created/removed

        private Guid? _defaultRecipeUid;
        public Guid? DefaultRecipeUid { get => _defaultRecipeUid; set => SetProperty(ref _defaultRecipeUid, value); }
        public RecipeItemViewModel? DefaultRecipe => DefaultRecipeUid != null ? _store.Recipes[(Guid)DefaultRecipeUid] : null;

        // Info updating
        protected override Dictionary<string, Action<ResourceDto>> ConfigureUpdaters() => new()
        {
            { nameof(ResourceDto.Name), dto => Name = dto.Name },
            { nameof(ResourceDto.DefaultRecipeUid), dto => DefaultRecipeUid = dto.DefaultRecipeUid },
        };

        private void OnResourceUpdated(ResourceUpdatedEvent ev)
        {
            if (Uid != ev.Resource.Uid) return;

            Update(ev.Resource, ev.ChangedProperties);
        }

        private void OnRecipeCreated(RecipeCreatedEvent ev)
        {
            if (Uid != ev.Recipe.ParentResourceUid) return;

            var recipeVM = _partsFactory.CreateRecipeVM(ev.Recipe);
            Recipes.Add(recipeVM);
        }

        private void OnRecipeDeleted(RecipeDeletedEvent ev)
        {
            if (Uid != ev.ParentResourceUid) return;

            var recipeVM = Recipes.FirstOrDefault(r => r.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                Recipes.Remove(recipeVM);
                recipeVM.Dispose();
            }
        }

        private void OnRecipeMoved(RecipeMovedEvent ev)
        {
            if (Uid == ev.OldResourceUid)
            {
                var recipeVM = Recipes.FirstOrDefault(c => c.Uid == ev.RecipeUid);
                if (recipeVM != null)
                {
                    Recipes.Remove(recipeVM);
                }
            }
            else if (Uid == ev.NewResourceUid)
            {
                var recipeVM = _store.Recipes.GetValueOrDefault(ev.RecipeUid);
                if (recipeVM != null)
                {
                    Recipes.Add(recipeVM);
                }
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();

            foreach (var recipe in  Recipes)
                recipe.Dispose();

            _store.Resources.Remove(Uid);
        }
    }
}
