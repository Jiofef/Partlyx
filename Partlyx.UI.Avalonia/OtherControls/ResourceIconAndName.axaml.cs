using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.UI.Avalonia;

public partial class ResourceIconAndName : UserControl
{
    public static readonly StyledProperty<ResourceViewModel?> ResourceProperty =
    AvaloniaProperty.Register<ResourceIconAndName, ResourceViewModel?>(
        "Resource",
        defaultValue: null
        );

    public ResourceViewModel? Resource
    {
        get => GetValue(ResourceProperty);
        set => SetValue(ResourceProperty, value);
    }
    public ResourceIconAndName()
    {
        InitializeComponent();
    }
}