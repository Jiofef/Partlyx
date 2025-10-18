using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.Commands.ResourceCommonCommands
{
    public class CreateResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;
        private IPartlyxRepository _resourceRepository;

        public Guid ResourceUid { get; private set; }

        private Resource? _createdResource;

        public CreateResourceCommand(IResourceService rs, IPartlyxRepository rr)
        {
            _resourceService = rs;
            _resourceRepository = rr;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _resourceService.CreateResourceAsync();
            ResourceUid = uid;
            _createdResource = await _resourceRepository.GetResourceByUidAsync(uid);
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(ResourceUid);
            ResourceUid = Guid.Empty;
        }

        public async Task Redo()
        {
            if (_createdResource == null) return;

            await _resourceRepository.AddResourceAsync(_createdResource);
        }
    }

    public class DeleteResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;
        private IPartlyxRepository _resourceRepository;

        public Guid DeletedResourceUid { get; private set; }

        private Resource? _deletedResource;

        public DeleteResourceCommand(Guid resourceUid, IResourceService rs, IPartlyxRepository rr)
        {
            DeletedResourceUid = resourceUid;
            _resourceService = rs;
            _resourceRepository = rr;
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
            _deletedResource = null;
        }
    }

    public class DuplicateResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;

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
