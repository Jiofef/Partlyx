using Partlyx.Core;
using Partlyx.Core.Partlyx;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.CoreExtensions;
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
        public ResourceService(IPartlyxRepository repo, IEventBus bus)
        {
            _repo = repo;
            _eventBus = bus;
        }

        public async Task<Guid> CreateResourceAsync(string? name = null)
        {
            var resource = new Resource();
            if (name != null)
                resource.Name = name;

            resource.Name = await _repo.GetUniqueResourceNameAsync(resource.Name);

            var icon = new FigureIcon();
            var iconInfo = icon.GetInfo();
            resource.UpdateIconInfo(iconInfo);
            
            var Uid = await _repo.AddResourceAsync(resource);

            _eventBus.Publish(new ResourceCreatedEvent(resource.ToDto(), resource.Uid));

            return Uid;
        }
        public async Task<Guid> DuplicateResourceAsync(Guid uid)
        {
            var duplicateUid = await _repo.DuplicateResourceAsync(uid);

            var resource = await GetResourceAsync(duplicateUid);
            if (resource != null)
                _eventBus.Publish(new ResourceCreatedEvent(resource, resource.Uid));

            return duplicateUid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            await _repo.DeleteResourceAsync(uid);

            _eventBus.Publish(new ResourceDeletedEvent(uid, uid));
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

        public async Task SetDefaultRecipeAsync(Guid resourceUid, Guid? recipeUid)
        {
            var batchOptions = new PartlyxRepository.BatchIncludeOptions() { };

            if (recipeUid is Guid recipeUidNotNull)
            {
                await _repo.ExecuteWithBatchAsync([resourceUid], [recipeUidNotNull], [], batchOptions,
                batch =>
                {
                    var resource = batch.Resources[resourceUid];
                    var recipe = batch.Recipes[recipeUidNotNull];
                    resource.DefaultRecipe = recipe;

                    return Task.CompletedTask;
                });
            }
            else
            {
                await _repo.ExecuteWithBatchAsync([resourceUid], [], [], batchOptions,
                batch =>
                {
                    var resource = batch.Resources[resourceUid];
                    resource.DefaultRecipe = null;

                    return Task.CompletedTask;
                });
            }

            var resource = await GetResourceAsync(resourceUid);
            if (resource != null)
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[] { "DefaultRecipeUid" }, resource.Uid));
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
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[]{"Name"}, resource.Uid));
        }

        public async Task SetResourceIconAsync(Guid resourceUid, IconDto iconDto)
        {
            var iconInfo = iconDto.ToIconInfo();
            await _repo.ExecuteOnResourceAsync(resourceUid, resource =>
            {
                resource.UpdateIconInfo(iconInfo);
                return Task.CompletedTask;
            });

            var resource = await GetResourceAsync(resourceUid);
            if (resource != null)
                _eventBus.Publish(new ResourceUpdatedEvent(resource, new[] { "Icon" }, resource.Uid));
        }

        public async Task<int> GetResourcesCountAsync()
        {
            return await _repo.GetResourcesCountAsync();
        }
    }
}
