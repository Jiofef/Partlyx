using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace Partlyx.UI.Avalonia.Converters
{
    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color)
            {
                var color = (System.Drawing.Color)value;
                var avaloniaColor = new Color(color.A, color.R, color.G, color.B);
                return new SolidColorBrush(avaloniaColor);
            }
            return default!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var avaloniaColor = brush.Color;
                var systemColor = System.Drawing.Color.FromArgb(avaloniaColor.A, avaloniaColor.R, avaloniaColor.G, avaloniaColor.B);
                return systemColor;
            }
            return default!;
        }
    }
}
