using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public record PartsVMInitializationStartedEvent();
    public record PartsVMInitializationFinishedEvent();

    public class PartsInitializeService : IPartsInitializeService
    {
        private readonly IEventBus _bus;
        private readonly IVMPartsFactory _factory;
        private readonly IVMPartsStore _store;

        private readonly IDisposable _resourcesBulkLoadedSubscription;
        private readonly IDisposable _recipesBulkLoadedSubscription;
        private readonly IDisposable _recipeComponentsBulkLoadedSubscription;

        private readonly IDisposable _partsInitializationStartedSubscription;

        public bool IsResourcesLoaded { get; private set; }
        public bool IsRecipesLoaded { get; private set; }
        public bool IsRecipeComponentsLoaded { get; private set; }
        private bool _isEverythingLoaded => IsResourcesLoaded && IsRecipesLoaded && IsRecipeComponentsLoaded;
        public bool InitializationFinished { get; private set; }

        public PartsInitializeService(IEventBus bus, IVMPartsFactory vmpf, IVMPartsStore vmps)
        {
            _bus = bus;
            _factory = vmpf;
            _store = vmps;

            _resourcesBulkLoadedSubscription = bus.Subscribe<ResourcesBulkLoadedEvent>(OnResourceBulkLoaded, true);
            _recipesBulkLoadedSubscription = bus.Subscribe<RecipesBulkLoadedEvent>(OnRecipeBulkLoaded, true);
            _recipeComponentsBulkLoadedSubscription = bus.Subscribe<RecipeComponentsBulkLoadedEvent>(OnRecipeComponentBulkLoaded, true);

            _partsInitializationStartedSubscription = bus.Subscribe<PartsInitializationStartedEvent>(OnInitializationStarted, true);
        }

        private void OnInitializationStarted(PartsInitializationStartedEvent ev)
        {
            IsResourcesLoaded = IsRecipesLoaded = IsRecipeComponentsLoaded = InitializationFinished = false;
            _bus.Publish(new PartsVMInitializationStartedEvent());
        }

        private void OnResourceBulkLoaded(ResourcesBulkLoadedEvent ev)
        {
            foreach (var dto in ev.Bulk)
                _factory.GetOrCreateResourceVM(dto);

            IsResourcesLoaded = true;
            if (_isEverythingLoaded)
                OnLoadFinished();
        }

        private void OnRecipeBulkLoaded(RecipesBulkLoadedEvent ev)
        {
            foreach (var dto in ev.Bulk)
                _factory.GetOrCreateRecipeVM(dto);

            IsRecipesLoaded = true;
            if (_isEverythingLoaded)
                OnLoadFinished();
        }

        private void OnRecipeComponentBulkLoaded(RecipeComponentsBulkLoadedEvent ev)
        {
            foreach (var dto in ev.Bulk)
                _factory.GetOrCreateRecipeComponentVM(dto);

            IsRecipeComponentsLoaded = true;
            if (_isEverythingLoaded)
                OnLoadFinished();
        }

        private void RebuildTreeConnections()
        {
            foreach (var component in _store.RecipeComponents.Values)
            {
                var parentUid = component.ParentRecipeUid;
                if (parentUid == null) continue;

                if (_store.Recipes.TryGetValue((Guid)parentUid, out var parent))
                {
                    parent.InitAddChild(component);
                }
            }

            foreach (var recipe in _store.Recipes.Values)
            {
                var parentUid = recipe.ParentResourceUid;
                if (parentUid == null) continue;

                if (_store.Resources.TryGetValue((Guid)parentUid, out var parent))
                {
                    parent.InitAddChild(recipe);
                }
            }
        }

        private void OnLoadFinished()
        {
            RebuildTreeConnections();

            InitializationFinished = true;
            _bus.Publish(new PartsVMInitializationFinishedEvent());
        }

        public void Dispose()
        {
            _recipeComponentsBulkLoadedSubscription.Dispose();
            _recipesBulkLoadedSubscription.Dispose();
            _resourcesBulkLoadedSubscription.Dispose();

            _partsInitializationStartedSubscription.Dispose();
        }
    }
}
