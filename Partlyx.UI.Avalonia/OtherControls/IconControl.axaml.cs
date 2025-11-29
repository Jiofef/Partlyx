using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;

namespace Partlyx.UI.Avalonia;

public partial class IconControl : UserControl
{
    public static readonly StyledProperty<IconViewModel?> IconContentProperty =
        AvaloniaProperty.Register<IconControl, IconViewModel?>(
            "IconContent",
            defaultValue: null
            );

    public IconViewModel? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }
    public IconControl()
    {
        InitializeComponent();
    }
}