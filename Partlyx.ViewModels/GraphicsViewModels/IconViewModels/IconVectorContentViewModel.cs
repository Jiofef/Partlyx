using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.Dtos;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Drawing;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconVectorContentViewModel : ObservableObject, IIconContentViewModel
    {
        public IconVectorContentViewModel(FigureIconDto dto)
        {
            _figureType = dto.FigureType;
            _figureColor = dto.Color;
        }
        public IconVectorContentViewModel(string figureType, Color figureColor)
        {
            _figureType = figureType;
            _figureColor = figureColor;
        }
        public IconVectorContentViewModel(string figureType)
        {
            _figureType = figureType;
            _figureColor = StandardVisualSettings.StandardMainPartlyxColor;
        }

        private string _figureType;
        public string FigureType { get => _figureType; set => SetProperty(ref _figureType, value); }

        public string? GetFigureTypeName() => FigureType.ToString();

        private Color _figureColor;
        public Color FigureColor 
        { 
            get => _figureColor; 
            set 
            {
                if (_figureColor.ToArgb() != value.ToArgb())
                {
                    _figureColor = value;
                    OnPropertyChanged(nameof(FigureColor));
                }
            } 
        }

        public IconTypeEnumViewModel ContentIconType => IconTypeEnumViewModel.Vector;

        public IconVectorContentViewModel Clone() => new IconVectorContentViewModel(FigureType, FigureColor);
    }
}
