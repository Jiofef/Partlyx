using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Partlyx.Tests.DataTests
{
    [CollectionDefinition("InMemoryDB")]
    public class DBCollection : ICollectionFixture<DBFixture> { }

    public class DBFixture : IDisposable
    {
        public DBFixture() { }

        public ServiceProvider CreateProvider(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();

            services.AddSingleton<SqliteConnection>(sp => {
                var c = new SqliteConnection("DataSource=:memory:");
                c.Open();
                return c;
            });

            services.AddDbContextFactory<PartlyxDBContext>((sp, opt) =>
                opt.UseSqlite(sp.GetRequiredService<SqliteConnection>()));

            configure?.Invoke(services);

            var provider = services.BuildServiceProvider();

            using var ctx = provider.GetRequiredService<IDbContextFactory<PartlyxDBContext>>().CreateDbContext();
            ctx.Database.EnsureCreated();

            return provider; 
        }

        public void Dispose() { }
    }

}
