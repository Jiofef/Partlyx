using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class ResourceItemViewModel : UpdatableViewModel<ResourceDto>, IVMPart
    {
        // Services
        private readonly IPartsService _service;
        private readonly IVMPartsStore _store;
        private readonly IVMPartsFactory _partsFactory;
        private readonly IResourceItemUiStateService _uiStateService;
        
        // Events
        private readonly IEventBus _bus;
        private readonly IDisposable _updatedSubscription;
        private readonly IDisposable _childAddSubscription;
        private readonly IDisposable _childRemoveSubscription;
        private readonly IDisposable _childMoveSubscription;

        public ResourceItemViewModel(ResourceDto dto, IPartsService service, IVMPartsStore store, IVMPartsFactory partsFactory, IEventBus bus, IResourceItemUiStateService uiStateS)
        {
            Uid = dto.Uid;

            // Services
            _service = service;
            _store = store;
            _partsFactory = partsFactory;
            _bus = bus;
            _uiStateService = uiStateS;

            // Info
            _name = dto.Name;
            _defaultRecipeUid = dto.DefaultRecipeUid;

            foreach (var recipe in dto.Recipes)
            {
                var vm = _partsFactory.GetOrCreateRecipeVM(recipe);
                _recipes.Add(vm);
            }

            _defaultRecipe = _defaultRecipeUid != null ? _store.Recipes.GetValueOrDefault((Guid)_defaultRecipeUid) : null;

            // Info updating binding
            _updatedSubscription = _bus.Subscribe<ResourceUpdatedEvent>(OnResourceUpdated, true);
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
        public Guid? DefaultRecipeUid 
        {
            get => _defaultRecipeUid;
            set
            {
                SetProperty(ref _defaultRecipeUid, value);
                DefaultRecipe = value != null ? _store.Recipes.GetValueOrDefault((Guid)value) : null;
            }
        }

        [ObservableProperty]
        private RecipeItemViewModel? _defaultRecipe;

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

            var recipeVM = _partsFactory.GetOrCreateRecipeVM(ev.Recipe);
            Recipes.Add(recipeVM);

            if (ev.Recipe.Uid == DefaultRecipeUid)
                DefaultRecipe = recipeVM;
        }

        private void OnRecipeDeleted(RecipeDeletedEvent ev)
        {
            if (Uid != ev.ParentResourceUid) return;

            var recipeVM = Recipes.FirstOrDefault(r => r.Uid == ev.RecipeUid);
            if (recipeVM != null)
            {
                Recipes.Remove(recipeVM);
                recipeVM.Dispose();

                if (ev.RecipeUid == DefaultRecipeUid)
                    DefaultRecipe = null;
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

        /// <summary> Used when new DB is initialized and we need to connect created VM parts to each other </summary>
        internal void InitAddChild(RecipeItemViewModel recipe)
        {
            Recipes.Add(recipe);
        }

        public void Dispose()
        {
            _updatedSubscription.Dispose();
            _childAddSubscription.Dispose();
            _childRemoveSubscription.Dispose();
            _childMoveSubscription.Dispose();

            foreach (var recipe in  Recipes)
                recipe.Dispose();

            _store.Resources.Remove(Uid);
        }

        // For UI
        public ResourceItemUIState Ui => _uiStateService.GetOrCreate(this);
    }
}
