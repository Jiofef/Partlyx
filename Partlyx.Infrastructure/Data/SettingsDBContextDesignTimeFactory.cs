using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Partlyx.Infrastructure.Data
{
    public class SettingsDBContextDesignTimeFactory : IDesignTimeDbContextFactory<SettingsDBContext>
    {
        public SettingsDBContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<SettingsDBContext>()
                .UseSqlite($"Data Source={DirectoryManager.DefaultSettingsDBPath}")
                .Options;
            return new SettingsDBContext(options);
        }
    }
}
