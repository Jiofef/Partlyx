using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconImageContentViewModel : ObservableObject
    {
        public IconImageContentViewModel(ImageIconDto dto)
        {
            ImageUid = dto.ImageUid;
            _imageName = dto.ImageName;
        }

        public Guid ImageUid { get; }

        private string _imageName = "";
        public string ImageName { get => _imageName; set => SetProperty(ref _imageName, value); }
    }
}
