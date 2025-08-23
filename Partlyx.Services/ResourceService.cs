using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using Partlyx.Data;
using Partlyx.Services.Dtos;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Partlyx.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IDbContextFactory<PartlyxDBContext> _dbFactory;
        public ResourceService(IDbContextFactory<PartlyxDBContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Guid> CreateResourceAsync()
        {
            using var db = _dbFactory.CreateDbContext();

            var resource = new Resource();
            db.Resources.Add(resource);
            await db.SaveChangesAsync();
            return resource.Uid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.FindAsync(uid);
            if (r != null)
            {
                db.Resources.Remove(r);
                await db.SaveChangesAsync();
            }
        }

        public async Task<ResourceDto?> GetResourceAsync(Guid uid)
        {
            using var db = _dbFactory.CreateDbContext();

            var r = await db.Resources.Include(x => x.Recipes)
                .ThenInclude(rc => rc.Components)
                .FirstOrDefaultAsync(x => x.Uid == uid);

            return r == null ? null : r.ToDto();
        }

        public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
        {
            using var db = _dbFactory.CreateDbContext();

            var rl = await db.Resources.
                Where(r => EF.Functions.Like(r.Name, $"%{query}")).
                Select(r => r.ToDto()).
                ToListAsync();

            return rl;
        }
    }
}
