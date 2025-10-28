using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Converters
{
    public class SelectedEqualsConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = SelectedItem, values[1] = currentItem
            return Equals(values[0], values[1]);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

}
