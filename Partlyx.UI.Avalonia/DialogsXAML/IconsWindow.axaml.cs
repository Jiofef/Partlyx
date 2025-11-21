using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Partlyx.UI.Avalonia.DialogsXAML;

public partial class IconsWindow : UserControl
{
    public IconsWindow()
    {
        InitializeComponent();
    }

    private void FlexPanel_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }

    private void Image_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}