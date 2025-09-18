using CommunityToolkit.Mvvm.Input;
using Partlyx.Infrastructure.Data;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class MenuPanelFileViewModel
    {
        private IFileService _fileService;
        private IFileDialogService _dialogService;
        private INotificationService _notificationService;

        public MenuPanelFileViewModel(IFileService dbfm, IFileDialogService fds, INotificationService ns)
        {
            _fileService = dbfm;
            _dialogService = fds;
            _notificationService = ns;
        }

        [RelayCommand]
        public async Task NewFileAsync()
        {
            await _fileService.ClearCurrentFile();
        }

        [RelayCommand]
        public async Task SaveProjectAsync()
        {
            var lastExportDir = _fileService.CurrentPartreePath;

            if (lastExportDir == null)
                await SaveProjectAsAsync();
            else
            {
                var result = await _fileService.ExportPartreeAsync(lastExportDir);

                if (!result.Success)
                {
                    Trace.WriteLine("Partree export error");
                    await _notificationService.ShowErrorAsync("Saving error", "An error occured during saving a file. File wasn't saved");
                }
            }
        }

        [RelayCommand]
        public async Task SaveProjectAsAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync(new FileDialogOptions()
            {
                Title = "Open Project (.partree)",
                Filter = "Partree files (*.partree)|*.partree|All files (*.*)|*.*",
                DefaultFileName = "project.partree",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return;

            var result = await _fileService.ExportPartreeAsync(path);

            if (!result.Success)
            {
                Trace.WriteLine("Partree export error");
                await _notificationService.ShowErrorAsync("Saving error", "An error occured during saving a file. File wasn't saved");
            }
        }

        [RelayCommand]
        public async Task OpenProjectAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync(new FileDialogOptions()
            {
                Title = "Open Project (.partree)",
                Filter = "Partree files (*.partree)|*.partree|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return;

            var result = await _fileService.ImportPartreeAsync(path);

            if (!result.Success)
            {
                Trace.WriteLine("Partree import error");
                await _notificationService.ShowErrorAsync("Invalid file error", "An error occured during opening a file. File wasn't opened");
            }
        }
    }
}
