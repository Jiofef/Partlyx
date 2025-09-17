
using Partlyx.Infrastructure.Data.CommonFileEvents;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IDBProvider
    {
        string? ConnectionString { get; }
        bool IsInitialized { get; }
        string? CurrentDbPath { get; }
        SemaphoreSlim DBExportLoadSemaphore { get; }

        Task InitializeAsync(string dbPath, CancellationToken ct = default);
        void NotifyDatabaseReplaced();
    }
}
