using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IWorkingFileService
    {
        string? CurrentPartreePath { get; }
        bool IsChangesSaved { get; }

        Task ClearCurrentFile();
        Task DeleteWorkingDB();
        Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default);
        Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default);
    }
}