using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIServices.Interfaces;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelSettingsViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        public MenuPanelSettingsViewModel(IDialogService ds, IServiceProvider provider) 
        {
            _dialogService = ds;
            _serviceProvider = provider;
        }

        [RelayCommand]
        public async Task OpenSettings()
        {
            var settings = _serviceProvider.GetRequiredService<ApplicationSettingsMenuViewModel>();

            await _dialogService.ShowDialogAsync(settings);
        }
    }
}