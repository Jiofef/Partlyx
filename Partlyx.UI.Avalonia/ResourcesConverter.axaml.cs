using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Partlyx.UI.Avalonia;

public partial class ResourcesConverter : UserControl
{
    public ResourcesConverter()
    {
        InitializeComponent();
    }

    private void Border_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }

    private void NumericUpDown_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}