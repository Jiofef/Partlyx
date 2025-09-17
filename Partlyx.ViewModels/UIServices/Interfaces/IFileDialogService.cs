using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IFileDialogService
    {
        /// <summary>Open one file. Returns path or null if cancelled.</summary>
        Task<string?> ShowOpenFileDialogAsync(FileDialogOptions options, CancellationToken ct = default);

        /// <summary>Save file. Returns path or null if cancelled.</summary>
        Task<string?> ShowSaveFileDialogAsync(FileDialogOptions options, CancellationToken ct = default);

        /// <summary>Chose a folder. Returns folder path or null.</summary>
        Task<string?> ShowSelectFolderDialogAsync(string? initialDirectory = null, CancellationToken ct = default);
    }

    public class FileDialogOptions
    {
        public string Title { get; init; } = "";
        public string Filter { get; init; } = "All files (*.*)|*.*";
        public string DefaultFileName { get; init; } = "";
        public string InitialDirectory { get; init; } = "";
        public bool CheckFileExists { get; init; } = true;
        public bool OverwritePrompt { get; init; } = true; // for Save
    }

}
