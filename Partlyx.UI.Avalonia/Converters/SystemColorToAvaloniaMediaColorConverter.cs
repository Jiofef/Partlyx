using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Converters
{
    public class SystemColorToAvaloniaMediaColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Drawing.Color)
            {
                var systemColor = (System.Drawing.Color)value;
                return new Color(systemColor.A, systemColor.R, systemColor.G, systemColor.B);
            }
            return default!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color avaloniaColor)
            {
                return System.Drawing.Color.FromArgb(avaloniaColor.A, avaloniaColor.R, avaloniaColor.G, avaloniaColor.B);
            }
            return default!;
        }
    }
}
