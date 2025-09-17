using Partlyx.Infrastructure.Data.Implementations;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IDBSaver
    {
        Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default);
    }
}