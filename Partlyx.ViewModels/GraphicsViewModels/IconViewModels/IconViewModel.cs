using CommunityToolkit.Mvvm.ComponentModel;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconViewModel : ObservableObject
    {
        private IconTypeEnumViewModel _iconType = IconTypeEnumViewModel.Null;
        public IconTypeEnumViewModel IconType { get => _iconType; set => SetProperty(ref _iconType, value); }

        private IIconContentViewModel? _content;
        public IIconContentViewModel? Content { get => _content; set => SetProperty(ref _content, value); }
    }
}
