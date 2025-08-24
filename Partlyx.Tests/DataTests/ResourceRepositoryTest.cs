using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Data;
using System;

namespace Partlyx.Tests
{
    public class ResourceRepositoryTest
    {
        private ServiceCollection _services;

        public ResourceRepositoryTest()
        {
            _services = new ServiceCollection();
            TestDBInitializer.InitTestDB(_services);
            _services.AddTransient<IResourceRepository, ResourceRepository>();
        }

        [Fact]
        public async Task CreateAndGetResourceAsync_CreateEmptyResource_GetItFromDBAndCheckCorrectness()
        {
            // Arrange
            var provider = _services.BuildServiceProvider();
            var resourceRepo = provider.GetRequiredService<IResourceRepository>();

            var resource = new Resource("SomeResource");

            // Act
            Guid resourceUid = await resourceRepo.AddAsync(resource);
            var dbResource = await resourceRepo.GetByUidAsync(resourceUid);

            // Assert
            Assert.NotNull(dbResource);
            Assert.Equal(resource.Uid, dbResource!.Uid);
        }
    }

}