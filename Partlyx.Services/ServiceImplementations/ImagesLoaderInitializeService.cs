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
        private IDisposable _fileClosedSubscription;

        public bool IsImagesLoaded { get; private set; } = false;

        public ImagesLoaderInitializeService(IEventBus bus, IImagesRepository repo)
        {
            _bus = bus;
            _repo = repo;

            _partlyxDbInitializeSubscription = _bus.SubscribeAsync<PartlyxDBInitializedEvent>(OnDBInitialized);
            _fileClosedSubscription = _bus.Subscribe<FileClosedEvent>(_ => IsImagesLoaded = false);
        }

        private async Task OnDBInitialized(PartlyxDBInitializedEvent ev)
        {
            await _bus.PublishAsync(new ImagesDBInitializationStartedEvent());
            var images = await _repo.GetAllTheImagesAsync(true, false);

            var dtos = images.Select(x => x.ToDto()).ToArray();
            await _bus.PublishAsync(new ImagesBulkLoadedEvent(dtos));

            await _bus.PublishAsync(new ImagesDBInitializationFinishedEvent());

            IsImagesLoaded = true;
        }

        public void Dispose()
        {
            _partlyxDbInitializeSubscription.Dispose();
            _fileClosedSubscription.Dispose();
        }
    }
}
