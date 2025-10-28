using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Services.Dtos;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconViewModel : ObservableObject
    {
        public IconViewModel(IconDto dto) 
        {
            UpdateFromDto(dto);
        }
        private IconTypeEnumViewModel _iconType = IconTypeEnumViewModel.Figure;
        public IconTypeEnumViewModel IconType { get => _iconType; set => SetProperty(ref _iconType, value); }

        private object? _content;
        public object? Content { get => _content; set => SetProperty(ref _content, value); }

        public void UpdateFromDto(IconDto dto)
        {
            if (dto is ImageIconDto imageDto)
            {
                Content = new IconImageContentViewModel(imageDto);
                IconType = IconTypeEnumViewModel.Image;
            }
            else if (dto is FigureIconDto figureDto)
            {
                Content = new IconFigureContentViewModel(figureDto);
                IconType = IconTypeEnumViewModel.Figure;
            }
            else
            {
                Content = new IconNullContentViewModel();
                IconType = IconTypeEnumViewModel.Null;
            }
        }
    }
}
