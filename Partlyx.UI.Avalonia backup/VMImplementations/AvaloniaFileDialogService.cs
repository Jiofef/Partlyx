using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Partlyx.UI.Avalonia.Helpers.UIThreadHelper;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaFileDialogService : IFileDialogService
    {
        public Task<string?> ShowOpenFileDialogAsync(FileDialogOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var dlg = new OpenFileDialog()
                {
                    Title = options.Title,
                    Filter = options.Filter,
                    FileName = options.DefaultFileName,
                    CheckFileExists = options.CheckFileExists,
                    InitialDirectory = options.InitialDirectory
                };

                var owner = GetOwnerWindow();
                var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
                return result == true ? dlg.FileName : null;
            }); 
        }

        public Task<string?> ShowSaveFileDialogAsync(FileDialogOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var dlg = new SaveFileDialog()
                {
                    Title = options.Title,
                    Filter = options.Filter, 
                    FileName = options.DefaultFileName,
                    InitialDirectory = options.InitialDirectory,
                    OverwritePrompt = options.OverwritePrompt
                };

                var owner = GetOwnerWindow();
                var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
                return result == true ? dlg.FileName : null;
            });
        }

        public Task<string?> ShowSelectFolderDialogAsync(string? initialDirectory = null, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var dlg = new OpenFolderDialog()
                {
                    InitialDirectory = initialDirectory
                };

                var owner = GetOwnerWindow();
                var result = dlg.ShowDialog();
                return result == true ? dlg.SafeFolderName : null;
            });
        }

        private Window? GetOwnerWindow() => Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }
}
