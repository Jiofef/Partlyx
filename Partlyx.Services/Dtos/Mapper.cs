using Partlyx.Core;
using System.Linq;

namespace Partlyx.Services.Dtos
{
    public static class Mapper
    {
        public static ResourceDto ToDto(this Resource r)
        {
            return new ResourceDto(
                r.Id,
                r.Name,
                r.Recipes.Select(rc => rc.ToDto()).ToList(),
                r.Recipes.ToList().IndexOf(r.DefaultRecipe)
                );
        }

        public static RecipeDto ToDto(this Recipe recipe)
        {
            return new RecipeDto(
                recipe.Components.Select(c => c.ToDto()).ToList()
                );
        }

        public static RecipeComponentDto ToDto(this RecipeComponent c)
        {
            return new RecipeComponentDto(
                c.ComponentResource.Id,
                c.ComponentResource.Name,
                c.Quantity,
                c.ComponentResource.Recipes.ToList().IndexOf(c.ComponentSelectedRecipe)
                );
        }
    }
}
