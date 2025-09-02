using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data;
using Partlyx.Services;
using Partlyx.Tests.DataTests;

namespace Partlyx.Tests
{
    [Collection("InMemoryDB")]
    public class ResourceServiceTest : IDisposable
    {
        private ServiceProvider _provider;

        public ResourceServiceTest(DBFixture fixture) 
        {
            _provider = fixture.CreateProvider(
                services =>
                {
                    services.AddTransient<IResourceService, ResourceService>();
                    services.AddTransient<IResourceRepository, ResourceRepository>();
                });
        }
        public void Dispose() => _provider.Dispose();

        [Fact]
        public async void CreateAndGetResourceAsync_CreateEmptyResource_CheckItsExistence()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IResourceRepository>();
            var resourceService = _provider.GetRequiredService<IResourceService>();

            // Act
            Guid resourceUid = await resourceService.CreateResourceAsync();
            var resource = await resourceService.GetResourceAsync(resourceUid);

            // Assert
            Assert.NotNull(resource);
        }

        [Fact]
        public async void GetAllTheResourcesAsync_CreateThreeResources_GetResourcesCountEquals3()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IResourceRepository>();
            var resourceService = _provider.GetRequiredService<IResourceService>();

            // Act
            for (int i = 0; i < 3; i++)
                await resourceService.CreateResourceAsync();
            var resources = await resourceService.GetAllTheResourcesAsync();

            // Assert
            Assert.Equal(3, resources.Count);
        }

        [Fact]
        public async void SetNameAsync_CreateResourceAndSetNameAsTestObject_GetNameTestObject()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IResourceRepository>();
            var resourceService = _provider.GetRequiredService<IResourceService>();

            var uid = await resourceService.CreateResourceAsync();

            // Act
            await resourceService.SetNameAsync(uid, "TestObject");
            var resource = await resourceService.GetResourceAsync(uid);

            // Assert
            Assert.Equal("TestObject", resource!.Name);
        }
    }

}