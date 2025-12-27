using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceImplementations;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.ResourceCommonCommands
{
    public class CreateResourceCommand : IUndoableCommand
    {
        private readonly IResourceService _resourceService;
        private readonly PartsCreatorService _partsCreator;
        private readonly IPartlyxRepository _repo;

        public Guid ResourceUid { get; private set; }

        private Resource? _createdResource;
        private readonly string? _resourceName;

        public CreateResourceCommand(IResourceService rs, IEventBus bus, IPartlyxRepository repo, string? resourceName = null)
        {
            _resourceService = rs;
            _partsCreator = new PartsCreatorService(repo, bus);
            _repo = repo;
            _resourceName = resourceName;
        }

        public async Task ExecuteAsync()
        {
            ResourceUid = await _resourceService.CreateResourceAsync(_resourceName);
            // Get the created entity for potential redo
            _createdResource = await _repo.GetResourceByUidAsync(ResourceUid);
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(ResourceUid);
            ResourceUid = Guid.Empty;
        }

        public async Task RedoAsync()
        {
            if (_createdResource != null)
            {
                // Use PartsCreatorService to recreate the exact same resource entity
                ResourceUid = await _partsCreator.CreateResourceAsync(_createdResource);
            }
        }
    }

    public class DeleteResourceCommand : IUndoableCommand
    {
        private readonly IResourceService _resourceService;
        private readonly PartsCreatorService _partsCreator;
        private readonly IPartlyxRepository _repo;

        public Guid DeletedResourceUid { get; }

        private Resource? _deletedResource;

        public DeleteResourceCommand(Guid resourceUid, IResourceService rs, IEventBus bus, IPartlyxRepository repo)
        {
            DeletedResourceUid = resourceUid;
            _resourceService = rs;
            _partsCreator = new PartsCreatorService(repo, bus);
            _repo = repo;
        }

        public async Task ExecuteAsync()
        {
            _deletedResource = await _repo.GetResourceByUidAsync(DeletedResourceUid);
            await _resourceService.DeleteResourceAsync(DeletedResourceUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedResource != null)
            {
                await _partsCreator.CreateResourceAsync(_deletedResource);
                _deletedResource = null;
            }
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

    public class SetResourceIconCommand : SetValueUndoableCommand<IconDto>
    {
        private SetResourceIconCommand(IconDto newValue, IconDto savedValue, Func<IconDto, Task> setter)
            : base(newValue, savedValue, setter!) { }

        public static async Task<SetResourceIconCommand?> CreateAsync(IServiceProvider serviceProvider, Guid resourceUid, IconDto icon)
        {
            var resourceService = serviceProvider.GetRequiredService<IResourceService>();

            var res = await resourceService.GetResourceAsync(resourceUid);
            if (res == null)
                throw new ArgumentNullException("Resource doesn't exist with Uid: " + resourceUid);

            IconDto previousName = res.Icon;

            return new SetResourceIconCommand(icon, previousName, async (icon) =>
            {
                await resourceService.SetResourceIconAsync(resourceUid, icon);
            });
        }
    }
}
