using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.CoreExtensions;
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
                r.DefaultRecipeUid,
                r.GetIconInfo().ToDto()
                );
        }

        public static RecipeDto ToDto(this Recipe rc)
        {
            return new RecipeDto(
                rc.Uid,
                rc.ParentResource?.Uid,
                rc.Name,
                rc.CraftAmount,
                rc.Components.Select(c => c.ToDto()).ToList(),
                rc.GetIconInfo().ToDto()
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

        public static IconDto ToDto(this IconInfo ii)
        {
            switch (ii.Type)
            {
                case IconTypeEnum.Figure:
                    var figureIcon = (FigureIcon)ii.GetIcon();
                    return new FigureIconDto(figureIcon.Color, figureIcon.FigureType);
                case IconTypeEnum.Image:
                    var imageIcon = (ImageIcon)ii.GetIcon();
                    return new ImageIconDto(imageIcon.Uid, imageIcon.Name);
                default:
                    return new NullIconDto();
            }
        }
    }
}
