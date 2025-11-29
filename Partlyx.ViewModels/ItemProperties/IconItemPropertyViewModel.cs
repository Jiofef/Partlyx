using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Partlyx;
using Partlyx.ViewModels.DragAndDrop.Implementations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;

namespace Partlyx.ViewModels.ItemProperties
{
    public partial class IconItemPropertyViewModel : ItemPropertyViewModel
    {
        // Might be used later to create fast icon selection by dropping an image on the property or part item
        private DropHandlerBaseViewModel? _dropHandler;
        public DropHandlerBaseViewModel? DropHandler { get => _dropHandler; set => SetProperty(ref _dropHandler, value); }


        public Func<object?, Task>? SelectButtonPressedTask { get; set; }
        public IconItemPropertyViewModel(IconViewModel icon) 
        {
            _icon = icon;
        }
        private IconViewModel _icon;
        public IconViewModel Icon { get => _icon; set => SetProperty(ref _icon, value); }

        [RelayCommand]
        public async Task SelectButtonPressed(object? args)
        {
            if (SelectButtonPressedTask != null)
                await SelectButtonPressedTask(args);
        }
    }
}
