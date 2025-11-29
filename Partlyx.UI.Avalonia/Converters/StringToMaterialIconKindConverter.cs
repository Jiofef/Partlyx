using Avalonia.Data.Converters;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Converters
{
    public class StringToMaterialIconKindConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string iconName && Enum.TryParse<MaterialIconKind>(iconName, out var iconKind))
            {
                return iconKind;
            }
            return MaterialIconKind.QuestionMark;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }
}
