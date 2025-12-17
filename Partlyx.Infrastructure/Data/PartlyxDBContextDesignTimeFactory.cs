using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data
{
    public class PartlyxDBContextDesignTimeFactory : IDesignTimeDbContextFactory<PartlyxDBContext>
    {
        public PartlyxDBContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<PartlyxDBContext>()
                .UseSqlite($"Data Source={DirectoryManager.DefaultDBPath}")
                .Options;
            return new PartlyxDBContext(options);
        }
    }
}
