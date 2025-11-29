using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public partial class ImagesStoreViewModel : PartlyxObservable, IImagesStoreViewModel
    {
        public ObservableCollection<ImageViewModel> Images { get; } = new();

        private readonly Dictionary<Guid, ImageViewModel> _imagesDic = new();
        public ReadOnlyDictionary<Guid, ImageViewModel> ImagesDic { get; }

        private readonly IPartlyxImageService _imageService;
        private readonly ImageFactoryViewModel _factory;
        public ImagesStoreViewModel(IPartlyxImageService imageService, IEventBus bus, ImageFactoryViewModel imageFactory)
        {
            _imageService = imageService;
            _factory = imageFactory;

            ImagesDic = new(_imagesDic);

            var imageLoadedSubscription = bus.SubscribeAsync<ImageAddedToDbEvent>(OnImageAdded);
            Disposables.Add(imageLoadedSubscription);

            var imageRemovedSubscription = bus.SubscribeAsync<ImageDeletedFromDbEvent>(OnImageRemoved);
            Disposables.Add(imageRemovedSubscription);

            var fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => ClearStore(), true);
        }

        private async Task OnImageAdded(ImageAddedToDbEvent ev)
        {
            var imageDto = await _imageService.GetImageOrNullAsync(ev.ImageUid);

            if (imageDto == null)
                return;

            AddImageFromDto(imageDto);
        }
        private async Task OnImageRemoved(ImageDeletedFromDbEvent ev)
        {
            RemoveImage(ev.ImageUid);
        }

        public void AddImageFromDto(ImageDto dto)
        {
            var image = _factory.CreateImageViewModel(dto);

            Images.Add(image);
            _imagesDic.Add(image.Uid, image);
        }
        public void RemoveImage(Guid uid)
        {
            var image = _imagesDic.GetValueOrDefault(uid);
            if (image != null)
            {
                Images.Remove(image);
                image.Dispose();
            }
            else
            {
                var image2 = Images.FirstOrDefault(i => i.Uid == uid);
                if (image2 != null)
                {
                    Images.Remove(image2);
                    image2.Dispose();
                }
            }

            _imagesDic.Remove(uid); 
        }
        public void ClearStore()
        {
            _imagesDic.Clear();
            Images.Clear();
        }

        public ImageViewModel? GetImageOrNull(Guid uid)
            => ImagesDic.GetValueOrDefault(uid);

        // Original image loading section
        private const int MAX_CACHED_FULL_IMAGES_DEFAULT_AMOUNT = 96;
        private HashSet<Guid> _cachedFullImagesHashed = new HashSet<Guid>();
        private List<Guid> _cachedFullImages = new List<Guid>();

        private int _maxCachedFullImagesAmount = MAX_CACHED_FULL_IMAGES_DEFAULT_AMOUNT;
        public int MaxCachedFullImagesAmount 
        {
            get => _maxCachedFullImagesAmount; 
            set 
            {
                SetProperty(ref _maxCachedFullImagesAmount, value);
                ClearExcessImagesCache();
            } 
        }
        private void ClearExcessImagesCache(int? expectedCachedImagesAmount = null)
        {
            // If we need to clear the cache after resizing its max size, we don't need to specify the argument and it will clear the items depending on the cache excess at the moment.
            if (expectedCachedImagesAmount == null)
                expectedCachedImagesAmount = _cachedFullImages.Count;

            int _expectedCachedImagesAmount = (int)expectedCachedImagesAmount;
            int excessImagesAmount = int.Min(_expectedCachedImagesAmount - MaxCachedFullImagesAmount, _cachedFullImages.Count);

            if (excessImagesAmount <= 0)
                return;

            for(int i = 0; i < excessImagesAmount; i++)
            {
                var imageUid = _cachedFullImages[i];
                _cachedFullImagesHashed.Remove(imageUid);
                var image = ImagesDic.GetValueOrDefault(imageUid);
                if (image == null) continue;

                image.Original = null;
            }
            _cachedFullImages.RemoveRange(0, excessImagesAmount - 1);
        }
        public async Task LoadFullImages(params Guid[] imageUids)
        {
            int expectedCachedImagesAmount = _cachedFullImages.Count + imageUids.Length;
            if (expectedCachedImagesAmount > MaxCachedFullImagesAmount)
                ClearExcessImagesCache(expectedCachedImagesAmount);

            foreach (var uid in imageUids)
            {
                if (_cachedFullImagesHashed.Contains(uid)) continue; // We don't want to load an image that is cached already

                var image = ImagesDic.GetValueOrDefault(uid);
                if (image == null) continue;

                var content = await _imageService.GetFullImageOrNullAsync(uid);
                image.Original = content;

                _cachedFullImages.Add(image.Uid);
                _cachedFullImagesHashed.Add(image.Uid);

                if (_cachedFullImages.Count > MaxCachedFullImagesAmount)
                    break;
            }
        }
    }
}
