using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.ServiceImplementations;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class ImagesInitializeServiceViewModel : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IImagesStoreViewModel _store;

        private readonly IDisposable _imagesBulkLoadedSubscription;

        public ImagesInitializeServiceViewModel(IEventBus bus, IImagesStoreViewModel store)
        {
            _bus = bus;
            _store = store;

            _imagesBulkLoadedSubscription = _bus.Subscribe<ImagesBulkLoadedEvent>(OnImagesBulkLoaded);
        }

        private void OnImagesBulkLoaded(ImagesBulkLoadedEvent ev)
        {
            foreach (var imageDto in ev.Images)
                _store.AddImageFromDto(imageDto);
        }

        public void Dispose()
        {
            _imagesBulkLoadedSubscription?.Dispose();
        }
    }
}
