using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Data;
using Partlyx.Services;

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

        [Fact]
        public async void CreateAndGetResourceAsync_CreateEmptyResource_GetItFromDB()
        {
            // Arrange
            services.AddDbContextFactory<PartlyxDBContext>(opts => opts.UseInMemoryDatabase("test"));

            var provider = services.BuildServiceProvider();
            var resourceService = provider.GetRequiredService<IResourceService>();

            // Act
            int resourceId = await resourceService.CreateResourceAsync();
            var resource = await resourceService.GetResourceAsync(resourceId);

            // Assert
            Assert.Null(resource);
        }
    }

}