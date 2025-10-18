using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
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

        private readonly IPartsLoader _partsLoader;
        private readonly IPartsInitializeService _partsInitializeService;
        private readonly IVMFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IVMPartsStoreCleaner _cleaner;

        public MainViewModel(
            PartsTreeViewModel partsTree,
            PartsGraphViewModel partsGraph,
            ItemPropertiesViewModel itemProperties,
            MenuPanelViewModel menuPanel,
            IGlobalSelectedParts selectedParts,
            IPartsLoader pl,
            IPartsInitializeService pis,
            IVMFileService vmfs,
            INotificationService ns,
            IVMPartsStoreCleaner vmpsc
            )
        {
            PartsTree = partsTree;
            PartsGraph = partsGraph;
            ItemProperties = itemProperties;
            MenuPanel = menuPanel;
            SelectedParts = selectedParts;

            _partsLoader = pl;
            _partsInitializeService = pis;
            _fileService = vmfs;
            _notificationService = ns;
            _cleaner = vmpsc;
        }

        private const bool DISABLE_DB_DELETE_ON_EXIT = true; // During development, it is inconvenient to reopen the file every time you restart. However, don't forget to disable this when releasing the application.
        public async Task<bool> ConfirmClosingAsync()
        {
            // Saving changes notification
            if (!_fileService.IsChangesSaved)
            {
                bool? questionResult = await _notificationService.ShowYesNoCancelConfirmAsync(NotificationPresets.ExitingFileSaveConfirm);

                if (questionResult == true) // If answer is Yes
                {
                    var savingResult = await _fileService.SaveProjectAsync();
                    if (!savingResult)
                        return false;
                }
                else if (questionResult == null)// If answer is Cancel
                    return false;

                // If answer is No, we delete DB and close the app
                if (!DISABLE_DB_DELETE_ON_EXIT)
                    await _fileService.DeleteWorkingDBAsync();
                return true;
            }

            if (!DISABLE_DB_DELETE_ON_EXIT)
                await _fileService.DeleteWorkingDBAsync();
            return true;
        }
        public Task OnAppClosingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
