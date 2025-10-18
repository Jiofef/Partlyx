using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Commands;
using Partlyx.Services.Commands.ResourceCommonCommands;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.Tests.DataTests;

namespace Partlyx.Tests.InfrastructureTests
{
    [Collection("InMemoryDB")]
    public class ResourceCommonCommandsTest : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ICommandFactory _factory;
        private readonly IPartUpdater _updater;

        public ResourceCommonCommandsTest(DBFixture fixture) 
        {
            _provider = fixture.CreateProvider(services =>
            {
                services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
                services.AddSingleton<IEventBus, EventBus>();
                services.AddTransient<IPartlyxRepository, PartlyxRepository>();
                services.AddTransient<IResourceService, ResourceService>();
                services.AddTransient<IPartUpdater, PartUpdater>();

                services.AddTransient<ICommandFactory, DICommandFactory>();

                services.AddTransient<CreateResourceCommand>();
                services.AddTransient<DeleteResourceCommand>();
                services.AddTransient<DuplicateResourceCommand>();
                services.AddTransient<SetDefaultRecipeToResourceCommand>();
                services.AddTransient<SetNameToResourceCommand>();

                services.AddSingleton<Infrastructure.Events.IEventBus, Infrastructure.Events.EventBus>();
            });

            _factory = _provider.GetRequiredService<ICommandFactory>();
            _updater = _provider.GetRequiredService<IPartUpdater>();
            _dispatcher = _provider.GetRequiredService<ICommandDispatcher>();
        }
        public void Dispose() => _provider.Dispose();

        [Fact]
        public async Task CreateResource_CreateEmptyResource_FindCreatedResource()
        {
            // Arrange
            var resourceService = _provider.GetRequiredService<IResourceService>();
            var command = _factory.Create<CreateResourceCommand>();

            // Act
            await _dispatcher.ExcecuteAsync(command);
            var resource = await resourceService.GetResourceAsync(command.ResourceUid);

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(command.ResourceUid, resource.Uid);
        }

        [Fact]
        public async Task CreateResource_CreateResourceAndUndo_CheckIfResourceDeleted()
        {
            // Arrange
            var resourceService = _provider.GetRequiredService<IResourceService>();
            var command = _factory.Create<CreateResourceCommand>();

            // Act
            await _dispatcher.ExcecuteAsync(command);
            await _dispatcher.UndoAsync();
            var resource = await resourceService.GetResourceAsync(command.ResourceUid);

            // Assert
            Assert.Null(resource);
        }

        [Fact]
        public async Task CreateResource_CreateResourceThenUndoAndRedo_FindRecreatedResource()
        {
            // Arrange
            var resourceService = _provider.GetRequiredService<IResourceService>();
            var command = _factory.Create<CreateResourceCommand>();

            // Act
            await _dispatcher.ExcecuteAsync(command);
            await _dispatcher.UndoAsync();
            await _dispatcher.RedoAsync();
            var resource = await resourceService.GetResourceAsync(command.ResourceUid);

            // Assert
            Assert.NotNull(resource);
            Assert.Equal(command.ResourceUid, resource.Uid);
        }

        [Fact]
        public async Task SetNameToResource_SetResourceNameTwiceThenUndoThenRedo_CheckIfSettedNamesIsActual()
        {
            // Arrange
            var resourceService = _provider.GetRequiredService<IResourceService>();

            var createResourceCommand = _factory.Create<CreateResourceCommand>();
            await _dispatcher.ExcecuteAsync(createResourceCommand);

            var resUid = createResourceCommand.ResourceUid;
            var resource = await resourceService.GetResourceAsync(resUid);

            // Act & Assert
            var name1SetterCommand = await _factory.CreateAsync<SetNameToResourceCommand>(resUid, "Name1");
            await _dispatcher.ExcecuteAsync(name1SetterCommand!);
            var name2SetterCommand = await _factory.CreateAsync<SetNameToResourceCommand>(resUid, "Name2");
            await _dispatcher.ExcecuteAsync(name2SetterCommand!);

            resource = await resourceService.GetResourceAsync(resUid);
            Assert.Equal("Name2", resource!.Name);

            await _dispatcher.UndoAsync();
            resource = await resourceService.GetResourceAsync(resUid);
            Assert.Equal("Name1", resource!.Name);

            await _dispatcher.RedoAsync();
            resource = await resourceService.GetResourceAsync(resUid);
            Assert.Equal("Name2", resource!.Name);
        }
    }
}
