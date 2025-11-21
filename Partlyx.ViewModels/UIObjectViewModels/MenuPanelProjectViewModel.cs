using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelProjectViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        public MenuPanelProjectViewModel(IDialogService ds, IServiceProvider provider)
        {
            _dialogService = ds;
            _serviceProvider = provider;
        }

        [RelayCommand]
        public async Task OpenIcons()
        {
            var images = _serviceProvider.GetRequiredService<IconsMenuViewModel>();

            await _dialogService.ShowDialogAsync(images);
        }
    }
}
