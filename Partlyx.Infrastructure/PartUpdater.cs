using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Data.Interfaces;

namespace Partlyx.Infrastructure
{
    /// <summary>
    /// Methods of this class return the up-to-date version of a part from the database, if it exists. If the part is not in the database, null is returned
    /// </summary>
    public class PartUpdater : IPartUpdater
    {
        private IPartlyxRepository _repo;
        public PartUpdater(IPartlyxRepository rr) => _repo = rr;

        public async Task<Resource?> Update(Resource resource)
        {
            var uid = resource.Uid;
            var result = await _repo.GetResourceByUidAsync(uid);

            return result;
        }

        public async Task<Recipe?> Update(Recipe recipe)
        {
            var uid = recipe.Uid;
            var result = await _repo.GetRecipeByUidAsync(uid);

            return result;
        }

        public async Task<RecipeComponent?> Update(RecipeComponent component)
        {
            var uid = component.Uid;
            var parentRecipeUid = component.ParentRecipe?.Uid;

            if (parentRecipeUid == null) return null;

            var actualParent = await _repo.GetRecipeByUidAsync((Guid)parentRecipeUid);
            var result = actualParent?.GetRecipeComponentByUid(uid);

            return result;
        }
    }
}
