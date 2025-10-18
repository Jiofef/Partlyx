using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
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
                services.AddTransient<IPartlyxRepository, PartlyxRepository>();
            });
        }
        public void Dispose() => _provider.Dispose();

        [Fact]
        public async Task CreateAndGetResourceAsync_CreateEmptyResource_GetItFromDBAndCheckCorrectness()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IPartlyxRepository>();
            var resource = new Resource("SomeResource");

            // Act
            Guid resourceUid = await resourceRepo.AddResourceAsync(resource);
            var dbResource = await resourceRepo.GetResourceByUidAsync(resourceUid);

            // Assert
            Assert.NotNull(dbResource);
            Assert.Equal(resource.Uid, dbResource!.Uid);
        }
    }

}