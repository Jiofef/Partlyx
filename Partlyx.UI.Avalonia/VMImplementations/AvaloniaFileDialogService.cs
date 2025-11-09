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
            if (string.IsNullOrWhiteSpace(filter))
                return null;

            var parts = filter.Split('|');
            var list = new List<FilePickerFileType>();

            for (int i = 0; i < parts.Length - 1; i += 2)
            {
                var name = parts[i].Trim();
                var patternPart = parts[i + 1];

                var patterns = ParsePatternPart(patternPart);
                if (patterns.Count == 0)
                    continue;

                list.Add(new FilePickerFileType(string.IsNullOrEmpty(name) ? "Files" : name)
                {
                    Patterns = patterns
                });
            }

            return list.Count == 0 ? null : list;
        }

        private static List<string> ParsePatternPart(string part)
        {
            var separators = new[] { ';', ',' };
            var raw = part.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(s => !string.IsNullOrEmpty(s));
            var patterns = new List<string>();

            foreach (var p in raw)
            {
                var pat = p;
                if (pat == "*.*")
                {
                    patterns.Add("*.*");
                    continue;
                }
                if (pat.StartsWith("*"))
                {
                    patterns.Add(pat);
                    continue;
                }
                if (pat.StartsWith("."))
                {
                    patterns.Add("*" + pat);
                    continue;
                }
                if (pat.Contains('.'))
                {
                    patterns.Add("*" + (pat.StartsWith(".") ? pat : "." + pat));
                    continue;
                }
                patterns.Add("*." + pat.TrimStart('.'));
            }
            return patterns.Distinct().ToList();
        }
        private TopLevel? GetTopLevel() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
    }
}
