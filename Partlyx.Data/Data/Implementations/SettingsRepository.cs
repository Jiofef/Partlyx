using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Partlyx.Core;
using Partlyx.Core.Settings;
using Partlyx.Infrastructure.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IDbContextFactory<SettingsDBContext> _dbFactory;

        public SettingsRepository(IDbContextFactory<SettingsDBContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<int> AddOptionAsync(OptionEntity option)
        {
            await using var db = _dbFactory.CreateDbContext();

            db.Options.Add(option);
            await db.SaveChangesAsync();
            return option.Id;
        }

        public async Task DeleteOptionAsync(string key)
        {
            await using var db = _dbFactory.CreateDbContext();
            var r = await db.Options.FirstOrDefaultAsync(r => r.Key == key);

            if (r != null)
            {
                db.Options.Remove(r);
                await db.SaveChangesAsync();
            }
        }
        public async Task DeleteOptionByIdAsync(int id)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Id == id);

            if (o != null)
            {
                db.Options.Remove(o);
                await db.SaveChangesAsync();
            }
        }

        public async Task<OptionEntity?> GetOptionByIdAsync(int id)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Id == id);
            return o;
        }
        public async Task<OptionEntity?> GetOptionAsync(string key)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Key == key);
            return o;
        }

        public async Task<List<OptionEntity>> GetAllOptionsAsync()
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.ToListAsync();
            return o;
        }

        public async Task<string?> GetOptionValueAsync(string key)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Key == key);
            return o?.ValueJson;
        }

        public async Task<string?> GetDeserializedOptionValueStringAsync(string key)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Key == key);
            return JsonSerializer.Deserialize<string>(o?.ValueJson ?? "{}");
        }

        public async Task SetOptionJsonValueAsync(string key, string value)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Key == key);

            if (o != null)
            {
                o.ValueJson = value;
                await db.SaveChangesAsync();
            }
        }

        public async Task<OptionEntity?> SetOptionJsonValueAndGetItAsync(string key, string value)
        {
            await using var db = _dbFactory.CreateDbContext();
            var o = await db.Options.FirstOrDefaultAsync(r => r.Key == key);

            if (o != null)
            {
                o.ValueJson = value;
                await db.SaveChangesAsync();
                return o;
            }
            return default;
        }
    }
}
