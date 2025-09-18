using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IFileService
    {
        string? CurrentPartreePath { get; }

        Task ClearCurrentFile();
        Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default);
        Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default);
    }
}