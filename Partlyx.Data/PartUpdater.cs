using Partlyx.Core;
using Partlyx.Infrastructure.Data.Interfaces;

namespace Partlyx.Infrastructure
{
    /// <summary>
    /// Methods of this class return the up-to-date version of a part from the database, if it exists. If the part is not in the database, null is returned
    /// </summary>
    public class PartUpdater : IPartUpdater
    {
        private IPartsRepository _repo;
        public PartUpdater(IPartsRepository rr) => _repo = rr;

        public async Task<Resource?> Update(Resource resource)
        {
            var uid = resource.Uid;
            var result = await _repo.GetByUidAsync(uid);

            return result;
        }

        public async Task<Recipe?> Update(Recipe recipe)
        {
            var uid = recipe.Uid;
            var parentUid = recipe.ParentResource?.Uid;

            if (parentUid == null) return null;

            var actualParent = await _repo.GetByUidAsync((Guid)parentUid);
            var result = actualParent?.GetRecipeByUid(uid);

            return result;
        }

        public async Task<RecipeComponent?> Update(RecipeComponent component)
        {
            var uid = component.Uid;
            var grandParentUid = component.ParentRecipe?.ParentResource?.Uid;

            if (grandParentUid == null) return null;

            var actualGrandParent = await _repo.GetByUidAsync((Guid)grandParentUid);
            var result = actualGrandParent?.GetRecipeComponentByUid(uid);

            return result;
        }
    }
}
