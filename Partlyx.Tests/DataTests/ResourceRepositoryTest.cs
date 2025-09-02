using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data;
using Partlyx.Tests.DataTests;
using System;

namespace Partlyx.Tests
{
    [Collection("InMemoryDB")]
    public class ResourceRepositoryTest : IDisposable
    {
        private ServiceProvider _provider;

        public ResourceRepositoryTest(DBFixture fixture)
        {
            _provider = fixture.CreateProvider( services =>
            {
                services.AddTransient<IResourceRepository, ResourceRepository>();
            });
        }
        public void Dispose() => _provider.Dispose();

        [Fact]
        public async Task CreateAndGetResourceAsync_CreateEmptyResource_GetItFromDBAndCheckCorrectness()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IResourceRepository>();
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