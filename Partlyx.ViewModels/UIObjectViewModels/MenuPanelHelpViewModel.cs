using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Help;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelHelpViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        public MenuPanelHelpViewModel(IDialogService ds, IServiceProvider provider)
        {
            _dialogService = ds;
            _serviceProvider = provider;
        }

        [RelayCommand]
        public async Task OpenHelp()
        {
            var mainSection = InfoScheme.ApplicationHelp.MainSection;

            await TryOpenHelpWith(mainSection);
        }

        [RelayCommand]
        public async Task OpenAboutUs()
        {
            var aboutUs = _serviceProvider.GetRequiredService<AboutUsWindowViewModel>();

            await _dialogService.ShowDialogAsync(aboutUs);
        }

        public async Task<bool> TryOpenHelpWith(InfoSection section)
        {
            var help = _serviceProvider.GetRequiredService<HelpWindowViewModel>();
            var neededSection = help.Info.GetDescendantSectionOrNull(section.Key);
            if (neededSection == null)
                return false;

            help.SelectedItem = neededSection;
            await _dialogService.ShowDialogAsync(help);
            return true;
        }
    }
}
