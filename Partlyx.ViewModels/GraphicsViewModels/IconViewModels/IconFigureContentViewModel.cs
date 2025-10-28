using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Services.Dtos;
using System.Drawing;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconFigureContentViewModel : ObservableObject
    {
        public IconFigureContentViewModel(FigureIconDto dto)
        {
            _figureType = dto.FigureType;
            _figureColor = dto.Color;
        }

        private object _figureType;
        public object FigureType { get => _figureType; set => SetProperty(ref _figureType, value); }

        private Color _figureColor;
        public Color FigureColor { get => _figureColor; set => SetProperty(ref _figureColor, value); }
    }
}
