using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.ResourceCommonCommands
{
    public class CreateResourceCommand : IUndoableCommand
    {
        private readonly IResourceService _resourceService;
        private readonly IPartlyxRepository _resourceRepository;
        private readonly IEventBus _bus;

        public Guid ResourceUid { get; private set; }

        private Resource? _createdResource;
        private string? _resourceName;

        public CreateResourceCommand(IResourceService rs, IPartlyxRepository rr, IEventBus bus, string? name = null)
        {
            _resourceService = rs;
            _resourceRepository = rr;
            _bus = bus;
            _resourceName = name;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _resourceService.CreateResourceAsync(_resourceName);
            ResourceUid = uid;
            _createdResource = await _resourceRepository.GetResourceByUidAsync(uid);
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(ResourceUid);
            ResourceUid = Guid.Empty;
        }

        public async Task RedoAsync()
        {
            if (_createdResource == null) return;

            await _resourceRepository.AddResourceAsync(_createdResource);

            var @event = new ResourceCreatedEvent(_createdResource.ToDto());
            _bus.Publish(@event);
        }
    }

    public class DeleteResourceCommand : IUndoableCommand
    {
        private readonly IResourceService _resourceService;
        private readonly IPartlyxRepository _resourceRepository;
        private readonly IEventBus _bus;

        public Guid DeletedResourceUid { get; private set; }

        private Resource? _deletedResource;

        public DeleteResourceCommand(Guid resourceUid, IResourceService rs, IPartlyxRepository rr, IEventBus bus)
        {
            DeletedResourceUid = resourceUid;
            _resourceService = rs;
            _resourceRepository = rr;
            _bus = bus;
        }

        public async Task ExecuteAsync()
        {
            _deletedResource = await _resourceRepository.GetResourceByUidAsync(DeletedResourceUid);
            await _resourceService.DeleteResourceAsync(DeletedResourceUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedResource == null) return;

            await _resourceRepository.AddResourceAsync(_deletedResource);

            var @event = new ResourceCreatedEvent(_deletedResource.ToDto());
            _bus.Publish(@event);

            _deletedResource = null;
        }
    }

    public class DuplicateResourceCommand : IUndoableCommand
    {
        private readonly IResourceService _resourceService;

        private Guid _resourceUid;

        public Guid DuplicateUid { get; private set; }

        public DuplicateResourceCommand(Guid resourceUid, IResourceService rs)
        {
            _resourceService = rs;
            _resourceUid = resourceUid;
        }

        public async Task ExecuteAsync()
        {
            DuplicateUid = await _resourceService.DuplicateResourceAsync(_resourceUid);
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(DuplicateUid);
            DuplicateUid = Guid.Empty;
        }
    }

    public class SetDefaultRecipeToResourceCommand : SetValueUndoableCommand<Guid>
    {
        private SetDefaultRecipeToResourceCommand(Guid recipeUid, Guid previousRecipeUid, Func<Guid, Task> setter)
            : base(recipeUid, previousRecipeUid, setter) { }

        public static async Task<SetDefaultRecipeToResourceCommand?> CreateAsync(IServiceProvider serviceProvider, Guid resourceUid, Guid recipeUid)
        {
            var resourceService = serviceProvider.GetRequiredService<IResourceService>();

            var res = await resourceService.GetResourceAsync(resourceUid);
            if (res == null)
                throw new ArgumentNullException("Resource doesn't exist with Uid: " + resourceUid);

            if (res.DefaultRecipeUid == null)
                throw new Exception($"Trying to set a null default recipe Uid. Resource's uid: {res.Uid}");

            var previousRecipeUid = (Guid)res.DefaultRecipeUid;

            return new SetDefaultRecipeToResourceCommand(recipeUid, previousRecipeUid, async (uid) =>
            {
                await resourceService.SetDefaultRecipeAsync(resourceUid, uid);
            });
        }
    }

    public class SetNameToResourceCommand : SetValueUndoableCommand<string>
    {
        private SetNameToResourceCommand(string newName, string previousName, Func<string, Task> setter)
            : base(newName, previousName, setter!) { }

        public static async Task<SetNameToResourceCommand?> CreateAsync(IServiceProvider serviceProvider, Guid resourceUid, string newName)
        {
            var resourceService = serviceProvider.GetRequiredService<IResourceService>();

            var res = await resourceService.GetResourceAsync(resourceUid);
            if (res == null)
                throw new ArgumentNullException("Resource doesn't exist with Uid: " + resourceUid);

            string previousName = res.Name ?? "";

            return new SetNameToResourceCommand(newName, previousName, async (name) =>
            {
                await resourceService.SetNameAsync(resourceUid, name);
            });
        }
    }
}
