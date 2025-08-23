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
                recipe.CraftAmount,
                recipe.Components.Select(c => c.ToDto()).ToList()
                );
        }

        public static RecipeComponentDto ToDto(this RecipeComponent c)
        {
            return new RecipeComponentDto(
                c.ComponentResource.Uid,
                c.ComponentResource.Name,
                c.Quantity,
                c.ComponentSelectedRecipe.Uid
                );
        }
    }
}
