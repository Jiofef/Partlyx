using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class DBProvider : IDBProvider
    {
        private readonly IEventBus _bus;
        private readonly IServiceProvider _services;

        public SemaphoreSlim DBExportLoadSemaphore { get; private set; }
        
        public DBProvider(IEventBus bus, IServiceProvider services)
        {
            _bus = bus;
            _services = services;

            DBExportLoadSemaphore = new SemaphoreSlim(1, 1);
        }

        private string? currentDbPath;
        public string? CurrentDbPath { get => currentDbPath; }
        public string? ConnectionString => CurrentDbPath != null ? $"Data Source={CurrentDbPath}" : null;

        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync(string dbPath, CancellationToken ct = default)
        {
            if (IsInitialized && string.Equals(CurrentDbPath, dbPath, StringComparison.OrdinalIgnoreCase))
                return;

            currentDbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath));

            using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync(ct);

            // Enabling WAL
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            await cmd.ExecuteNonQueryAsync(ct);

            using var scope = _services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PartlyxDBContext>>();
            using var ctx = factory.CreateDbContext();
            ctx.Database.EnsureCreated();

            IsInitialized = true;

            NotifyDatabaseReplaced();
        }

        /// <summary>
        /// Call when you replace the database with a CurrentDbPath link and make sure it is correct.
        /// </summary>
        public void NotifyDatabaseReplaced()
        {
            _bus.Publish(new PartlyxDBInitializedEvent(CurrentDbPath!));
        }

        public void NotifyDatabaseClosed()
        {
            _bus.Publish(new FileClosedEvent());
        }
    }
}
