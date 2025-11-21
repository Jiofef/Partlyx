using Partlyx.Services.Dtos;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public interface IImagesStoreViewModel
    {
        ObservableCollection<ImageViewModel> Images { get; }
        ReadOnlyDictionary<Guid, ImageViewModel> ImagesDic { get; }
        int MaxCachedFullImagesAmount { get; set; }

        void AddImageFromDto(ImageDto dto);
        void Dispose();
        ImageViewModel? GetImageOrNull(Guid uid);
        Task LoadFullImages(params Guid[] imageUids);
    }
}
