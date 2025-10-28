using Avalonia.Data.Converters;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Converters
{
    public class ImageTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IconTypeEnumViewModel type)) return Visibility.Collapsed;
            var param = parameter as string;
            if (param == "Image") return type == IconTypeEnumViewModel.Image ? Visibility.Visible : Visibility.Collapsed;
            if (param == "Figure") return type == IconTypeEnumViewModel.Figure ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
