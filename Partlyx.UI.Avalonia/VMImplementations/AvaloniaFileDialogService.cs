using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Partlyx.UI.Avalonia.Helpers.UIThreadHelper;
namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaFileDialogService : IFileDialogService
    {
        public async Task<string?> ShowOpenFileDialogAsync(FileDialogOptions options, CancellationToken ct = default)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;
            var pickerOptions = new FilePickerOpenOptions
            {
                Title = options.Title,
                FileTypeFilter = ConvertFilters(options.Filter),
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(options.InitialDirectory ?? ""),
                AllowMultiple = false
            };
            var result = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions);
            return result?.FirstOrDefault()?.Path.LocalPath;
        }
        public async Task<string?> ShowSaveFileDialogAsync(FileDialogOptions options, CancellationToken ct = default)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;
            var pickerOptions = new FilePickerSaveOptions
            {
                Title = options.Title,
                FileTypeChoices = ConvertFilters(options.Filter),
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(options.InitialDirectory ?? ""),
                SuggestedFileName = options.DefaultFileName
            };
            var result = await topLevel.StorageProvider.SaveFilePickerAsync(pickerOptions);
            return result?.Path.LocalPath;
        }
        public async Task<string?> ShowSelectFolderDialogAsync(string? initialDirectory = null, CancellationToken ct = default)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;
            var pickerOptions = new FolderPickerOpenOptions
            {
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory ?? "")
            };
            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(pickerOptions);
            return result?.FirstOrDefault()?.Path.LocalPath;
        }
        private static IReadOnlyList<FilePickerFileType>? ConvertFilters(string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return null;
            return filter.Split('|').Chunk(2).Select(x => new FilePickerFileType(x[0]) { Patterns = x[1].Split(';').Select(p => $"*.{p}").ToList() }).ToList();
        }
        private TopLevel? GetTopLevel() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
    }
}