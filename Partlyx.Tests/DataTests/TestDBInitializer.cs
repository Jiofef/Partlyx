using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Data;

namespace Partlyx.Tests;

internal class TestDBInitializer
{
    public static void InitTestDB(ServiceCollection services)
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        services.AddDbContextFactory<PartlyxDBContext>(opts => opts.UseSqlite(connection));

        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IDbContextFactory<PartlyxDBContext>>();
        using var ctx = factory.CreateDbContext();
        ctx.Database.EnsureCreated();
    }
}
