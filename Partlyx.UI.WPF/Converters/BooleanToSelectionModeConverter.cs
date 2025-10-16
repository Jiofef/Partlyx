using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

public class BooleanToSelectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? SelectionMode.Single : SelectionMode.Extended;
        }
        return SelectionMode.Single; 
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectionMode selectionMode)
        {
            return selectionMode == SelectionMode.Single;
        }
        return true;
    }
}