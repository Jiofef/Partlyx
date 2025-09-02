using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Infrastructure.Data;

namespace Partlyx.Services.Commands.ResourceCommonCommands
{
    public class CreateResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;

        public Guid ResourceUid { get; private set; }

        public CreateResourceCommand(IResourceService rs)
        {
            _resourceService = rs;
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _resourceService.CreateResourceAsync();
            ResourceUid = uid;
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(ResourceUid);
            ResourceUid = Guid.Empty;
        }
    }

    public class DeleteResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;

        public Resource? DeletedResource { get; private set; }

        public DeleteResourceCommand(Guid resourceUid, IResourceService rs, IResourceRepository rr)
        {
            _resourceUid = resourceUid;
            _resourceService = rs;
            _resourceRepository = rr;
        }

        public async Task ExecuteAsync()
        {
            DeletedResource = await _resourceRepository.GetByUidAsync(_resourceUid);
            await _resourceService.DeleteResourceAsync(_resourceUid);
        }

        public async Task UndoAsync()
        {
            if (DeletedResource == null) return;

            await _resourceRepository.AddAsync(DeletedResource);
            DeletedResource = null;
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
