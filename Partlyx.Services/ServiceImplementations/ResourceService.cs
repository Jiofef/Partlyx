using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using System.Runtime.CompilerServices;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceService : IResourceService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceService(IPartlyxRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        public async Task<Guid> CreateResourceAsync(string? name = null)
        {
            var resource = new Resource();
            if (name != null)
                resource.Name = name;

            var icon = new FigureIcon();
            var iconInfo = _infoProvider.GetInfo(icon);
            resource.SetIcon(icon, iconInfo);
            
            var Uid = await _repo.AddResourceAsync(resource);

            _eventBus.Publish(new ResourceCreatedEvent(resource.ToDto()));

            return Uid;
        }
        public async Task<Guid> DuplicateResourceAsync(Guid uid)
        {
            var duplicateUid = await _repo.DuplicateResourceAsync(uid);

            var resource = await GetResourceAsync(uid);
            if (resource != null)
                _eventBus.Publish(new ResourceCreatedEvent(resource));

            return duplicateUid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            await _repo.DeleteResourceAsync(uid);

            _eventBus.Publish(new ResourceDeletedEvent(uid));
        }

        public async Task<ResourceDto?> GetResourceAsync(Guid uid)
        {
            var resource = await _repo.GetResourceByUidAsync(uid);

            return resource == null ? null : resource.ToDto();
        }

        public async Task<List<ResourceDto>> GetAllTheResourcesAsync()
        {
            var resources = await _repo.GetAllTheResourcesAsync();

            var resourcesDto = resources.Select(x => x.ToDto()).ToList();

            return resourcesDto;
        }

        public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
        {
            var resourcesList = await _repo.SearchResourcesAsync(query);
            var resourcesDtoList = resourcesList.Select(x => x.ToDto()).ToList();

            return resourcesDtoList;
        }

        public async Task<List<Guid>> SearchResourcesUidsAsync(string query)
        {
            var resourcesList = await _repo.SearchResourcesAsync(query);
            var resourcesUids = resourcesList.Select(x => x.Uid).ToList();

            return resourcesUids;
        }

        public async Task SetDefaultRecipeAsync(Guid resourceUid, Guid recipeUid)
        {
            await _repo.ExecuteOnRecipeAsync(resourceUid, recipeUid, recipe =>
            {
                var resource = recipe.ParentResource!;
                resource.SetDefaultRecipe(recipe);

                return Task.CompletedTask;
            });

            var resource = await GetResourceAsync(resourceUid);
            if (resource != null)
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[] { "DefaultRecipeUid" }));
        }

        public async Task SetNameAsync(Guid resourceUid, string name)
        {
            await _repo.ExecuteOnResourceAsync(resourceUid, resource =>
            {
                resource.Name = name;
                return Task.CompletedTask;
            });

            var resource = await GetResourceAsync(resourceUid);
            if (resource != null)
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[]{"Name"}));
        }

        public async Task<int> GetResourcesCountAsync()
        {
            return await _repo.GetResourcesCountAsync();
        }
    }
}
