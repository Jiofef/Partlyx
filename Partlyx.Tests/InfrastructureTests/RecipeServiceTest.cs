using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Tests.DataTests;

namespace Partlyx.Tests
{
    [Collection("InMemoryDB")]
    public class RecipeServiceTest : IDisposable
    {
        private ServiceProvider _provider;

        public RecipeServiceTest(DBFixture fixture) 
        {
            _provider = fixture.CreateProvider(
                services =>
                {
                    services.AddTransient<IResourceService, ResourceService>();
                    services.AddTransient<IRecipeService, RecipeService>();
                    services.AddTransient<IResourceRepository, ResourceRepository>();
                    services.AddTransient<IIconInfoProvider, IconInfoProvider>();
                    services.AddTransient<IEventBus, EventBus>();
                    services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();
                });
        }
        public void Dispose() => _provider.Dispose();

        [Fact]
        public async void CreateAndGetRecipeAsync_CreateEmptyRecipe_CheckItsExistence()
        {
            // Arrange
            var resourceRepo = _provider.GetRequiredService<IResourceRepository>();
            var resourceService = _provider.GetRequiredService<IResourceService>();
            var recipeService = _provider.GetRequiredService<IRecipeService>();

            Guid resourceUid = await resourceService.CreateResourceAsync();

            // Act
            Guid recipeUid = await recipeService.CreateRecipeAsync(resourceUid);
            var resource = await resourceRepo.GetByUidAsync(resourceUid);
            var recipe = resource?.GetRecipeByUid(recipeUid);

            // Assert
            Assert.NotNull(recipe);
        }
    }
}