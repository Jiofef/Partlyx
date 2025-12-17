using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Settings;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class SettingsDBProvider : ISettingsDBProvider
    {
        private readonly IEventBus _bus;
        private readonly IServiceProvider _services;

        public SettingsDBProvider(IEventBus bus, IServiceProvider services)
        {
            _bus = bus;
            _services = services;
        }
        public string? ConnectionString => $"Data Source={DirectoryManager.DefaultSettingsDBPath}";

        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            if (IsInitialized)
                return;

            using var conn = new SqliteConnection(ConnectionString);
            await conn.OpenAsync(ct);

            // Enabling WAL
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            await cmd.ExecuteNonQueryAsync(ct);

            using var scope = _services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SettingsDBContext>>();
            using var ctx = factory.CreateDbContext();
            ctx.Database.Migrate();

            await BuildSettingsScheme(ctx);

            IsInitialized = true;

            _bus.Publish(new SettingsDBInitializedEvent());
        }

        private async Task BuildSettingsScheme(SettingsDBContext db)
        {
            var scheme = SettingsScheme.ApplicationSettings;

            // Removing unexisting options
            foreach (var option in db.Options)
            {
                if (!scheme.OptionsDictionary.ContainsKey(option.Key))
                    db.Options.Remove(option);
            }

            // Adding new options
            foreach (var schematicOption in scheme.Options)
            {
                if (!await db.Options.AnyAsync(o => o.Key == schematicOption.Key))
                {
                    var option = new OptionEntity()
                    {
                        Key = schematicOption.Key,
                        ValueJson = schematicOption.DefaultValueJson,
                        TypeName = schematicOption.TypeName
                    };

                    db.Options.Add(option);
                }
            }

            await db.SaveChangesAsync();
        }


        public async Task DataBaseMigrateAsync()
        {
            using var scope = _services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SettingsDBContext>>();
            using var ctx = factory.CreateDbContext();

            await ctx.Database.MigrateAsync();
        }
    }
}
