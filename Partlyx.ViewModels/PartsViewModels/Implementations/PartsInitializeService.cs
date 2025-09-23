using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public record PartsVMInitializationStartedEvent();
    public record PartsVMInitializationFinishedEvent();

    // Previously, this class created recipes and their components by itself, but it turned out to be a bug, since they are created in the constructors of their ancestors during initialization.
    // So the variables associated with loading recipes and components here are rudimentary, they are left for the sake of the dependency inversion.
    public class PartsInitializeService : IPartsInitializeService
    {
        private readonly IEventBus _bus;
        private readonly IVMPartsFactory _factory;
        private readonly IVMPartsStore _store;

        private readonly IDisposable _resourcesBulkLoadedSubscription;

        private readonly IDisposable _partsInitializationStartedSubscription;

        // At the moment, due to the particularities of the VM parts initialization, all these three fields from below become true simultaneously.
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

            _partsInitializationStartedSubscription = bus.Subscribe<PartsInitializationStartedEvent>(OnInitializationStarted, true);
        }

        private void OnInitializationStarted(PartsInitializationStartedEvent ev)
        {
            IsResourcesLoaded = IsRecipesLoaded = IsRecipeComponentsLoaded = InitializationFinished = false;
            _bus.Publish(new PartsVMInitializationStartedEvent());
        }

        private void OnResourceBulkLoaded(ResourcesBulkLoadedEvent ev)
        {
            // Resources and recipes VMs are creating their children by themselves from dto in constructor
            foreach (var dto in ev.Bulk)
                _factory.GetOrCreateResourceVM(dto);

            IsResourcesLoaded = true;
            IsRecipesLoaded = true;
            IsRecipeComponentsLoaded = true;

            if (_isEverythingLoaded || true)
                OnLoadFinished();
        }

        private void OnLoadFinished()
        {
            InitializationFinished = true;
            _bus.Publish(new PartsVMInitializationFinishedEvent());
        }

        public void Dispose()
        {
            _resourcesBulkLoadedSubscription.Dispose();

            _partsInitializationStartedSubscription.Dispose();
        }
    }
}
