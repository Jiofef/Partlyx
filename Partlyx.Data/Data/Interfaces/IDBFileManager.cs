using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IDBFileManager
    {
        string? CurrentPartreePath { get; }

        Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default);
        Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default);
    }
}