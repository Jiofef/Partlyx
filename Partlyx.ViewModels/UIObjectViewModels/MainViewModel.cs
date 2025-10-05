using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices;
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

        public MenuPanelViewModel MenuPanel { get; }

        private readonly IPartsLoader _partsLoader;
        private readonly IPartsInitializeService _partsInitializeService;
        private readonly IVMFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly IVMPartsStoreCleaner _cleaner;

        public MainViewModel(
            PartsTreeViewModel partsTree,
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
            MenuPanel = menuPanel;
            SelectedParts = selectedParts;

            _partsLoader = pl;
            _partsInitializeService = pis;
            _fileService = vmfs;
            _notificationService = ns;
            _cleaner = vmpsc;
        }

        public async Task<bool> ConfirmClosingAsync()
        {
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

                // If answer is No, we just close the app
                return true;
            }

            return true;
        }
        public Task OnAppClosingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
