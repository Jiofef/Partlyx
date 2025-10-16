using Partlyx.Core;
using System.Linq;

namespace Partlyx.Services.Dtos
{
    public static class Mapper
    {
        public static ResourceDto ToDto(this Resource r)
        {
            return new ResourceDto(
                r.Uid,
                r.Name,
                r.Recipes.Select(rc => rc.ToDto()).ToList(),
                r.DefaultRecipeUid
                );
        }

        public static RecipeDto ToDto(this Recipe recipe)
        {
            return new RecipeDto(
                recipe.Uid,
                recipe.ParentResource?.Uid,
                recipe.Name,
                recipe.CraftAmount,
                recipe.Components.Select(c => c.ToDto()).ToList()
                );
        }

        public static RecipeComponentDto ToDto(this RecipeComponent c)
        {
            return new RecipeComponentDto(
                c.Uid,
                c.ParentRecipe?.Uid,
                c.ComponentResource.Uid,
                c.Quantity,
                c.ComponentSelectedRecipeUid
                );
        }
    }
}
