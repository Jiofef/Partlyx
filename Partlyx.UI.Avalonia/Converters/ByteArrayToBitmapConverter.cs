using System;
using System.IO;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Partlyx.UI.Avalonia.Converters;

public class ByteArrayToBitmapConverter : IValueConverter
{
    // Singleton instance for easy usage from XAML via x:Static (if desired) or StaticResource.
    public static readonly ByteArrayToBitmapConverter Instance = new ByteArrayToBitmapConverter();

    /// <summary>
    /// Converts byte[] -> IBitmap.
    /// Returns null when input is null, or AvaloniaProperty.UnsetValue on error/wrong type.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        if (value is not byte[] bytes)
            return AvaloniaProperty.UnsetValue;

        try
        {
            // Wrap bytes in a MemoryStream and create a Bitmap. Bitmap reads the stream on construction,
            // so it's safe to dispose the stream immediately after creating the Bitmap.
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;
            return new Bitmap(ms);
        }
        catch
        {
            return AvaloniaProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ByteArrayToBitmapConverter does not support ConvertBack.");
    }
}
