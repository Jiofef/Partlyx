using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services;
using System;

namespace Partlyx.Tests
{
    public class ResourceServiceTest
    {
        private ServiceCollection _services;

        public ResourceServiceTest() 
        {
            _services = new ServiceCollection();
            TestDBInitializer.InitTestDB(_services);

            _services.AddTransient<Data.IResourceRepository, Data.ResourceRepository>();
            _services.AddTransient<IResourceService, ResourceService>();
        }

        [Fact]
        public async void CreateAndGetResourceAsync_CreateEmptyResource_CheckItsExistence()
        {
            // Arrange

            var provider = _services.BuildServiceProvider();
            var resourceRepo = provider.GetRequiredService<Data.IResourceRepository>();
            var resourceService = provider.GetRequiredService<IResourceService>();

            // Act
            Guid resourceUid = await resourceService.CreateResourceAsync();
            var resource = await resourceService.GetResourceAsync(resourceUid);

            // Assert
            Assert.NotNull(resource);
        }
    }

}