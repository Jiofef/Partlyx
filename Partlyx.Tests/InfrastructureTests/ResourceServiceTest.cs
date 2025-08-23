using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Data;
using Partlyx.Services;
using System;

namespace Partlyx.Tests
{
    public class ResourceServiceTest
    {
        private ServiceCollection services;

        public ResourceServiceTest() 
        {
            services = new ServiceCollection();
            services.AddTransient<IResourceService, ResourceService>();
        }

        private void InitDB()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContextFactory<PartlyxDBContext>(opts => opts.UseSqlite(connection));

            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<IDbContextFactory<PartlyxDBContext>>();
            using var ctx = factory.CreateDbContext();
            ctx.Database.EnsureCreated();
        }

        [Fact]
        public async Task CreateAndGetResourceAsync_CreateEmptyResource_GetItFromDB()
        {
            // Arrange
            InitDB();

            var provider = services.BuildServiceProvider();
            var resourceService = provider.GetRequiredService<IResourceService>();

            // Act
            Guid resourceUid = await resourceService.CreateResourceAsync();
            var resource = await resourceService.GetResourceAsync(resourceUid);

            // Assert
            Assert.NotNull(resource);
        }
    }

}