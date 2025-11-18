using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;

namespace Partlyx.Infrastructure.Data
{
    public static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddSingleton<IJsonInfoProvider, JsonInfoProvider>();
            services.AddTransient<IJsonLoader, JsonLoader>();
            services.AddTransient<IJsonSaver, JsonSaver>();

            // Partlyx DB setting
            services.AddSingleton<IPartlyxDBProvider, PartlyxDBProvider>();

            services.AddTransient<IDBLoader, DBLoader>();
            services.AddTransient<IDBSaver, DBSaver>();

            var dbDefaultPath = DirectoryManager.DefaultDBPath;
            var defaultDBConnectionString = @$"Data Source={dbDefaultPath}";

            services.AddDbContextFactory<PartlyxDBContext>((sp, options) =>
            {
                var connStr = sp.GetRequiredService<IPartlyxDBProvider>().ConnectionString ?? defaultDBConnectionString;

                options.UseSqlite(connStr);
            });

            // Settings DB setting
            services.AddSingleton<ISettingsDBProvider, SettingsDBProvider>();

            var settingsDbDefaultPath = DirectoryManager.DefaultSettingsDBPath;
            var defaultSettingsDBConnectionString = @$"Data Source = {settingsDbDefaultPath}";

            services.AddDbContextFactory<SettingsDBContext>((sp, options) =>
            {
                var connStr = sp.GetRequiredService<ISettingsDBProvider>().ConnectionString ?? defaultSettingsDBConnectionString;

                options.UseSqlite(connStr);
            });


            return services;
        }
    }
}
