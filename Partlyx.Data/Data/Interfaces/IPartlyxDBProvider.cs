
using Partlyx.Infrastructure.Data.CommonFileEvents;

namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface IPartlyxDBProvider
    {
        string? ConnectionString { get; }
        bool IsInitialized { get; }
        string? CurrentDbPath { get; }
        SemaphoreSlim DBExportLoadSemaphore { get; }

        Task DataBaseMigrateAsync();
        Task InitializeAsync(string dbPath, CancellationToken ct = default);
        void NotifyDatabaseClosed();
        void NotifyDatabaseReplaced();
    }
}
