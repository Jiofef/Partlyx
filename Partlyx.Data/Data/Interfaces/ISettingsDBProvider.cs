namespace Partlyx.Infrastructure.Data.Interfaces
{
    public interface ISettingsDBProvider
    {
        string? ConnectionString { get; }
        bool IsInitialized { get; }

        Task DataBaseMigrateAsync();
        Task InitializeAsync(CancellationToken ct = default);
    }
}