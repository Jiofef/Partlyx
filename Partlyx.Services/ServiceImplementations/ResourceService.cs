using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceService(IResourceRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        public async Task<Guid> CreateResourceAsync()
        {
            var resource = new Resource();
            var icon = new FigureIcon();
            var iconInfo = _infoProvider.GetInfo(icon);
            resource.SetIcon(icon, iconInfo);
            
            var Uid = await _repo.AddAsync(resource);

            _eventBus.Publish(new ResourceCreatedEvent(resource.ToDto()));

            return Uid;
        }
        public async Task<Guid> DuplicateResourceAsync(Guid uid)
        {
            var duplicateUid = await _repo.DuplicateAsync(uid);

            var resource = await GetResourceAsync(uid);
            if (resource != null)
                _eventBus.Publish(new ResourceCreatedEvent(resource));

            return duplicateUid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            await _repo.DeleteAsync(uid);

            _eventBus.Publish(new ResourceDeletedEvent(uid));
        }

        public async Task<ResourceDto?> GetResourceAsync(Guid uid)
        {
            var resource = await _repo.GetByUidAsync(uid);

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
            var resourcesList = await _repo.SearchAsync(query);
            var resourcesDtoList = resourcesList.Select(x => x.ToDto()).ToList();

            return resourcesDtoList;
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
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[] { "DefaultRecipe" }));
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
    }
}
