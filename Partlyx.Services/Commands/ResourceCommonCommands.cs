using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core;
using Partlyx.Data;

namespace Partlyx.Services.Commands.ResourceCommonCommands
{
    public class CreateResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;

        private Guid _resourceUid;

        public CreateResourceCommand(IServiceProvider serviceProvider)
        {
            _resourceService = serviceProvider.GetRequiredService<IResourceService>();
        }

        public async Task ExecuteAsync()
        {
            Guid uid = await _resourceService.CreateResourceAsync();
            _resourceUid = uid;
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(_resourceUid);
            _resourceUid = Guid.Empty;
        }
    }

    public class DeleteResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;
        private IResourceRepository _resourceRepository;

        private Guid _resourceUid;

        private Resource? _deletedResource;

        public DeleteResourceCommand(IServiceProvider serviceProvider, Guid resourceUid)
        {
            _resourceUid = resourceUid;
            _resourceService = serviceProvider.GetRequiredService<IResourceService>();
            _resourceRepository = serviceProvider.GetRequiredService<IResourceRepository>();
        }

        public async Task ExecuteAsync()
        {
            _deletedResource = await _resourceRepository.GetByUidAsync(_resourceUid);
            await _resourceService.DeleteResourceAsync(_resourceUid);
        }

        public async Task UndoAsync()
        {
            if (_deletedResource == null) return;

            await _resourceRepository.AddAsync(_deletedResource);
            _deletedResource = null;
        }
    }

    public class DuplicateResourceCommand : IUndoableCommand
    {
        private IResourceService _resourceService;

        private Guid _resourceUid;

        private Guid _duplicateUid;

        public DuplicateResourceCommand(IServiceProvider serviceProvider, Guid resourceUid)
        {
            _resourceService = serviceProvider.GetRequiredService<IResourceService>();
            _resourceUid = resourceUid;
        }

        public async Task ExecuteAsync()
        {
            _duplicateUid = await _resourceService.DuplicateResourceAsync(_resourceUid);
        }

        public async Task UndoAsync()
        {
            await _resourceService.DeleteResourceAsync(_duplicateUid);
            _duplicateUid = Guid.Empty;
        }
    }

    public class SetDefaultRecipeToResourceCommand : SetValueUndoableCommand<Guid>
    {
        public SetDefaultRecipeToResourceCommand(Guid recipeUid, Guid previousRecipeUid, Func<Guid, Task> setter)
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
        public SetNameToResourceCommand(string newName, string previousName, Func<string, Task> setter)
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
