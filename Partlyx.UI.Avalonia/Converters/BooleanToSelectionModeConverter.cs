using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Partlyx.UI.Avalonia.Converters;

public class BooleanToSelectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue ? SelectionMode.Single : SelectionMode.Multiple;
        }
        return SelectionMode.Single; 
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectionMode selectionMode)
        {
            return selectionMode == SelectionMode.Multiple;
        }
        return true;
    }
}