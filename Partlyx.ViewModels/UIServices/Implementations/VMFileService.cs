using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class VMFileService : IVMFileService
    {
        private IFileService _fileService;
        private IFileDialogService _dialogService;
        private INotificationService _notificationService;
        private IEventBus _bus;

        public VMFileService(IFileService dbfm, IFileDialogService fds, INotificationService ns, IEventBus bus)
        {
            _fileService = dbfm;
            _dialogService = fds;
            _notificationService = ns;
            _bus = bus;
        }

        public bool IsChangesSaved => _fileService.IsChangesSaved;

        public async Task NewFileAsync()
        {
            if (!IsChangesSaved)
            {
                bool? questionResult = await _notificationService.ShowYesNoCancelConfirmAsync(NotificationPresets.NewFileCreationFileSaveConfirm);

                if (questionResult == true) // If answer is Yes
                {
                    var savingResult = await SaveProjectAsync();
                    if (!savingResult)
                        return;
                }
                else if (questionResult == null)// If answer is Cancel
                    return;

                // If answer is No, we just continue
            }

            var @event = new FileClosedUIEvent();
            await _bus.PublishAsync(@event);

            await _fileService.ClearCurrentFile();
        }

        public async Task DeleteWorkingDBAsync()
        {
            var @event = new FileClosedUIEvent();
            await _bus.PublishAsync(@event);

            await _fileService.DeleteWorkingDB();
        }

        public async Task<bool> SaveProjectAsync()
        {
            var lastExportDir = _fileService.CurrentPartreePath;

            if (lastExportDir == null)
                return await SaveProjectAsAsync();
            else
            {
                var result = await _fileService.ExportPartreeAsync(lastExportDir);

                if (!result.Success)
                {
                    Trace.WriteLine("Partree export error");
                    await _notificationService.ShowErrorAsync(NotificationPresets.SavingFileErrorPreset);
                }
                return result.Success;
            }
        }

        public async Task<bool> SaveProjectAsAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync(new FileDialogOptions()
            {
                Title = "Open Project (.partree)",
                Filter = "Partree files (*.partree)|*.partree|All files (*.*)|*.*",
                DefaultFileName = "project.partree",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return false;

            var result = await _fileService.ExportPartreeAsync(path);

            if (!result.Success)
            {
                Trace.WriteLine("Partree export error");
                await _notificationService.ShowErrorAsync(NotificationPresets.SavingFileErrorPreset);
            }

            return result.Success;
        }

        public async Task OpenProjectAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync(new FileDialogOptions()
            {
                Title = "Open Project (.partree)",
                Filter = "Partree files (*.partree)|*.partree|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return;

            var @event = new FileClosedUIEvent();
            await _bus.PublishAsync(@event);

            var result = await _fileService.ImportPartreeAsync(path);

            if (!result.Success)
            {
                Trace.WriteLine("Partree import error");
                await _notificationService.ShowErrorAsync(NotificationPresets.InvalidFileErrorPreset);
            }
        }
    }
}
