using Microsoft.EntityFrameworkCore;
using Partlyx.Infrastructure.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public partial class PartlyxRepository : IPartlyxRepository
    {

        public async Task<string> GetUniqueResourceNameAsync(string baseName)
        {
            await using var db = _dbFactory.CreateDbContext();

            return await GetUniqueNameAsync(db.Resources, r => r.Name, baseName);
        }

        public async Task<string> GetUniqueRecipeNameAsync(string baseName)
        {
            await using var db = _dbFactory.CreateDbContext();

            return await GetUniqueNameAsync(db.Recipes, r => r.Name, baseName);
        }

        private async Task<string> GetUniqueNameAsync<T>(
            DbSet<T> dbSet,
            Expression<Func<T, string>> propertySelector,
            string baseName) where T : class
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Property selector must be a member access expression.");

            string propertyName = memberExpression.Member.Name;

            var existingNames = await dbSet
                .AsNoTracking()
                .Where(x => EF.Property<string>(x, propertyName).StartsWith(baseName))
                .Select(x => EF.Property<string>(x, propertyName))
                .ToListAsync();

            if (!existingNames.Contains(baseName))
                return baseName;

            var regex = new Regex($@"^{Regex.Escape(baseName)} (\d+)$");
            var suffixes = new HashSet<int>(
                existingNames
                    .Select(n => regex.Match(n))
                    .Where(m => m.Success)
                    .Select(m => int.Parse(m.Groups[1].Value))
            );

            int counter = 2;
            while (suffixes.Contains(counter))
            {
                counter++;
            }

            return $"{baseName} {counter}";
        }
    }
}
