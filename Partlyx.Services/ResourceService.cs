using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using Partlyx.Data;
using Partlyx.Services.Dtos;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Partlyx.Services
{
    public class ResourceService
    {
        private readonly IDbContextFactory<PartlyxDBContext> _dbFactory;
        public ResourceService(IDbContextFactory<PartlyxDBContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<int> CreateResourceAsync()
        {
            using var db = _dbFactory.CreateDbContext();

            var resource = new Resource();
            db.Resources.Add(resource);
            await db.SaveChangesAsync();
            return resource.Id;
        }

        public async Task DeleteResourceAsync(int id)
        {
            using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.FindAsync(id);
            if (r != null)
            {
                db.Resources.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        public async Task<ResourceDto?> GetResourceAsync(int id)
        {
            using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.Include(x => x.Recipes)
                .ThenInclude(rc => rc.Components)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            return r == null ? null : r.ToDto();
        }
    }
}
