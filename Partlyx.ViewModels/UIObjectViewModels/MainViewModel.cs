using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Contracts;
using Partlyx.Core.Help;
using Partlyx.Core.OtherSaves;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.Settings;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public class MainViewModel
    {
        public IGlobalSelectedParts SelectedParts { get; }

        public PartsTreeViewModel PartsTree { get; }

        public PartsGraphViewModel PartsGraph { get; }

        public ItemPropertiesViewModel ItemProperties { get; }

        public MenuPanelViewModel MenuPanel { get; }

        public IMainWindowController WindowController { get; }

        private readonly IPartsLoader _partsLoader;
        private readonly IPartsInitializeService _partsInitializeService;
        private readonly IVMFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IVMPartsStoreCleaner _cleaner;
        private readonly ILocalizationService _loc;
        private readonly IServicesResponsibilitySettingsHandler _servicesSettingsHandler;
        private readonly IGlobalApplicationSettingsServiceViewModelContainer _appSettingsContainer;
        private readonly IJsonSavesService _jsonSavesService;

        public MainViewModel(
            PartsTreeViewModel partsTree,
            PartsGraphViewModel partsGraph,
            ItemPropertiesViewModel itemProperties,
            MenuPanelViewModel menuPanel,
            IMainWindowController windowController,
            IGlobalSelectedParts selectedParts,
            IPartsLoader pl,
            IPartsInitializeService pis,
            IVMFileService vmfs,
            INotificationService ns,
            IVMPartsStoreCleaner vmpsc,
            ILocalizationService loc,
            IServicesResponsibilitySettingsHandler srsh,
            IGlobalApplicationSettingsServiceViewModelContainer gassvmc,
            IJsonSavesService jss
            )
        {
            PartsTree = partsTree;
            PartsGraph = partsGraph;
            ItemProperties = itemProperties;
            MenuPanel = menuPanel;
            SelectedParts = selectedParts;
            WindowController = windowController;

            _partsLoader = pl;
            _partsInitializeService = pis;
            _fileService = vmfs;
            _notificationService = ns;
            _cleaner = vmpsc;
            _loc = loc;
            _servicesSettingsHandler = srsh;
            _appSettingsContainer = gassvmc;
            _jsonSavesService = jss;
        }

        private const bool DISABLE_DB_DELETE_ON_EXIT = true; // During development, it is inconvenient to reopen the file every time you restart. However, don't forget to disable this when releasing the application.
        
        public async Task OnInitializeFinished()
        {
            var isMainAppJsonSchemeLoaded = await _jsonSavesService.WaitUntilSchemeIsLoaded(SaveScheme.AppUsingSave);

            if (!isMainAppJsonSchemeLoaded)
                return;

            var isAppStartedFirstTime = _jsonSavesService.GetValue<bool?>(SaveScheme.AppUsingSave.SchemeName, "is_first_time_opened");
            if (isAppStartedFirstTime == true)
            {
                await _jsonSavesService.SetValueAndSaveAsync(SaveScheme.AppUsingSave.SchemeName, "is_first_time_opened", false);

                // Opening help window with a greeting section
                await Task.Delay(1000);
                await MenuPanel.HelpMenu.OpenHelp();
            }
        }
        public async Task<bool> ConfirmClosingAsync()
        {
            // Saving changes notification
            bool isExitConfirmed = await _fileService.DropFileConfirmNotificationIfNeeded(NotificationPresets.ExitingFileSaveConfirm);

            if (isExitConfirmed)
            {
                if (!DISABLE_DB_DELETE_ON_EXIT)
                    await _fileService.DeleteWorkingDBAsync();
            }

            return isExitConfirmed;
        }
        public Task OnAppClosingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
