using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconServiceViewModel
    {
        private readonly IImagesLoaderInitializeService _imagesInitializerService;
        private readonly IImagesStoreViewModel _imagesStore;
        private readonly IEventBus _bus;

        // Only for creating a helper service for inherited icons
        private readonly IServiceProvider _provider;
        public IconServiceViewModel(IImagesLoaderInitializeService ins, IImagesStoreViewModel imagesStore, IEventBus bus, IServiceProvider provider)
        {
            _imagesInitializerService = ins;
            _imagesStore = imagesStore;
            _bus = bus;
            _provider = provider;
        }

        public async Task<IIconContentViewModel?> GetImageOrNullFromStoreAsync(Guid imageUid)
        {
            bool isImagesInitialized = _imagesInitializerService.IsImagesLoaded;
            IIconContentViewModel? result;
            if (isImagesInitialized)
            {
                result = _imagesStore.ImagesDic.GetValueOrDefault(imageUid);
            }
            else
            {
                // DB is not initialized yet, so we wait for it to finish initializing and then load our image
                await _bus.WaitUntil<ImagesDBInitializationFinishedEvent>();
                result = _imagesStore.ImagesDic.GetValueOrDefault(imageUid);
            }
            if (result == null)
                result = new IconNullContentViewModel();

            return result;
        }

        public async Task<IconViewModel> CreateFromDtoAsync(IconDto dto)
        {
            if (dto is FigureIconDto figureDto)
            {
                var content = new IconVectorContentViewModel(figureDto);
                return new IconViewModel { Content = content };
            }
            else if (dto is ImageIconDto imageIconDto)
            {
                var content = await GetImageOrNullFromStoreAsync(imageIconDto.ImageUid);
                return new IconViewModel { Content = content };
            }
            else if (dto is InheritedIconDto inheritedIconDto)
            {
                var service = _provider.GetRequiredService<InheritedIconHelperServiceViewModel>();
                var content = new InheritedIconContentViewModel(service);
                await content.FindAndSetParent(inheritedIconDto.ParentUid, inheritedIconDto.ParentType);
                return new IconViewModel { Content = content};
            }
            else
                return new IconViewModel() { Content = new IconNullContentViewModel() };
        }
    }
}
