using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels.UIStates
{
    public partial class ImageUiItemStateViewModel : ObservableObject
    {
        private readonly IPartlyxImageService _imageService;
        public ImageViewModel AttachedImage { get; }
        public ImageUiItemStateViewModel(ImageViewModel parentImage, IPartlyxImageService imageService)
        {
            _imageService = imageService;

            _unConfirmedName = parentImage.Name;

            AttachedImage = parentImage;
        }


        private bool _isRenaming;
        private string _unConfirmedName;

        public bool IsRenaming { get => _isRenaming; set => SetProperty(ref _isRenaming, value); }
        public string UnConfirmedName { get => _unConfirmedName; set => SetProperty(ref _unConfirmedName, value); }

        [RelayCommand]
        public async Task CommitNameChangeAsync()
        {
            if (!IsRenaming) return;

            await _imageService.SetImageNameAsync(AttachedImage.Uid, UnConfirmedName);

            IsRenaming = false;
        }

        [RelayCommand]
        public void CancelNameChange()
        {
            UnConfirmedName = AttachedImage.Name;
            IsRenaming = false;
        }

        [RelayCommand]
        public void StartRenaming()
            => IsRenaming = true;

        [RelayCommand]
        public async Task DeleteImage()
        {
            await _imageService.DeleteImageAsync(AttachedImage.Uid);
        }
    }
}
