using Partlyx.Core.Partlyx;

namespace Partlyx.Infrastructure
{
    /// <summary>
    /// Methods of this class should return the up-to-date version of a part from the database, if it exists. If the part is not in the database, null is returned
    /// </summary>
    public interface IPartUpdater
    {
        Task<Recipe?> Update(Recipe recipe);
        Task<RecipeComponent?> Update(RecipeComponent component);
        Task<Resource?> Update(Resource resource);
    }
}