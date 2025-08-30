using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using Partlyx.Data;
using Partlyx.Services.Dtos;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Partlyx.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _repo;
        public ResourceService(IResourceRepository repo) => _repo = repo;

        public async Task<Guid> CreateResourceAsync()
        {
            var resource = new Resource();
            var Uid = await _repo.AddAsync(resource);

            return Uid;
        }
        public async Task<Guid> DuplicateResourceAsync(Guid uid)
        {
            var duplicateUid = await _repo.DuplicateAsync(uid);

            return duplicateUid;
        }

        public async Task DeleteResourceAsync(Guid uid)
        {
            await _repo.DeleteAsync(uid);
        }

        public async Task<ResourceDto?> GetResourceAsync(Guid uid)
        {
            var resource = await _repo.GetByUidAsync(uid);

            return resource == null ? null : resource.ToDto();
        }

        internal async Task<Resource?> GetOriginalResourceAsync(Guid uid)
        {
            return await _repo.GetByUidAsync(uid);
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
        }

        public async Task SetNameAsync(Guid resourceUid, string name)
        {
            await _repo.ExecuteOnResourceAsync(resourceUid, resource =>
            {
                resource.Name = name;
                return Task.CompletedTask;
            });
        }
    }
}
