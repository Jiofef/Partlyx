using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class ImagesLoaderInitializeService : IImagesLoaderInitializeService
    {
        private readonly IEventBus _bus;
        private readonly IImagesRepository _repo;
        private IDisposable _partlyxDbInitializeSubscription;

        public ImagesLoaderInitializeService(IEventBus bus, IImagesRepository repo)
        {
            _bus = bus;
            _repo = repo;

            _partlyxDbInitializeSubscription = _bus.SubscribeAsync<PartlyxDBInitializedEvent>(OnDBInitialized);
        }

        private async Task OnDBInitialized(PartlyxDBInitializedEvent ev)
        {
            _bus.Publish(new ImagesDBInitializationStartedEvent());
            var images = await _repo.GetAllTheImagesAsync(true, false);

            var dtos = images.Select(x => x.ToDto()).ToArray();
            _bus.Publish(new ImagesBulkLoadedEvent(dtos));

            _bus.Publish(new ImagesDBInitializationFinishedEvent());
        }

        public void Dispose()
        {
            _partlyxDbInitializeSubscription.Dispose();
        }
    }
}
