using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Contracts;
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
        private readonly IFileService _fileService;
        private readonly IFileDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IEventBus _bus;
        private readonly MainWindowNameController _mainWindowNameController;
        private readonly ILocalizationService _loc;

        public VMFileService(IFileService dbfm, IFileDialogService fds, INotificationService ns, IEventBus bus, MainWindowNameController mwnc, ILocalizationService loc)
        {
            _fileService = dbfm;
            _dialogService = fds;
            _notificationService = ns;
            _bus = bus;
            _mainWindowNameController = mwnc;
            _loc = loc;
        }

        public bool IsChangesSaved => _fileService.IsChangesSaved;

        public async Task<bool> DropFileConfirmNotificationIfNeeded(NotificationConfirmOptions options)
        {
            if (!IsChangesSaved)
            {
                _mainWindowNameController.SetAsPostfix(_loc["postfix_you_forgot_to_save"]);
                bool? questionResult = await _notificationService.ShowYesNoCancelConfirmAsync(options);

                if (questionResult == true) // If answer is Yes
                {
                    var savingResult = await SaveProjectAsync();
                    if (!savingResult)
                    {
                        _mainWindowNameController.ClearPostfix();
                        return false;
                    }
                }
                else if (questionResult == null)// If answer is Cancel
                {
                    _mainWindowNameController.ClearPostfix();
                    return false;
                }

                _mainWindowNameController.ClearPostfix();
                return true; // If answer is No
            }

            _mainWindowNameController.ClearPostfix();
            return true;
        }

        public async Task NewFileAsync()
        {
            bool isOperationConfirmed = await DropFileConfirmNotificationIfNeeded(NotificationPresets.NewFileCreationFileSaveConfirm);
            if (!isOperationConfirmed)
                return;

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
                Title = _loc["partree_Save_Project"],
                Filter = _loc["filter_Partree_files"],
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
                Title = _loc["partree_Open_Project"],
                Filter = _loc["filter_Partree_files"],
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return;

            bool isOperationConfirmed = await DropFileConfirmNotificationIfNeeded(NotificationPresets.OtherFileOpenFileSaveConfirm);
            if (!isOperationConfirmed)
                return;

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
