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
            services.AddSingleton<IDBProvider, DBProvider>();

            services.AddTransient<IDBLoader, DBLoader>();
            services.AddTransient<IDBSaver, DBSaver>();

            var dbDefaultPath = DirectoryManager.DefaultDBPath;
            var defaultConnectionString = @$"Data Source={dbDefaultPath}";

            services.AddDbContextFactory<PartlyxDBContext>((sp, options) =>
            {
                var connStr = sp.GetRequiredService<IDBProvider>().ConnectionString ?? defaultConnectionString;

                options.UseSqlite(connStr);
            });


            return services;
        }
    }
}
