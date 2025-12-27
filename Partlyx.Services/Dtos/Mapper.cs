using Partlyx.Core;
using Partlyx.Core.Partlyx;
using Partlyx.Core.Settings;
using Partlyx.Core.Technical;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.CoreExtensions;
using Partlyx.Services.Helpers;
using System.Linq;
using System.Text.Json;

namespace Partlyx.Services.Dtos
{
    public static class Mapper
    {
        public static ResourceDto ToDto(this Resource r)
        {
            return new ResourceDto(
                r.Uid,
                r.Name,
                r.DefaultRecipeUid,
                r.GetIconInfo().ToDto()
                );
        }

        public static RecipeDto ToDto(this Recipe rc)
        {
            return new RecipeDto(
                rc.Uid,
                rc.Name,
                rc.IsReversible,
                rc.Inputs.Select(c => c.ToDto()).ToList(),
                rc.Outputs.Select(c => c.ToDto()).ToList(),
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
                c.IsOutput,
                c.ComponentSelectedRecipeUid
                );
        }

        public static IconDto ToDto(this IconInfo ii)
        {
            switch (ii.Type)
            {
                case IconTypeEnum.Null:
                    return new NullIconDto();
                case IconTypeEnum.Figure:
                    var figureIcon = (FigureIcon)ii.GetIcon();
                    return new FigureIconDto(figureIcon.Color, figureIcon.FigureType);
                case IconTypeEnum.Image:
                    var imageIcon = (ImageIcon)ii.GetIcon();
                    return new ImageIconDto(imageIcon.Uid);
                case IconTypeEnum.Inherited:
                    var inheritedIcon = (InheritedIcon)ii.GetIcon();
                    return new InheritedIconDto(inheritedIcon.Uid, inheritedIcon.ParentType);
                default:
                    return new NullIconDto();
            }
        }

        public static ImageDto ToDto(this PartlyxImage img)
        {
            return new ImageDto(
                img.Uid,
                img.Name,
                img.Mime,
                img.Hash,
                img.Content,
                img.CompressedContent
                );
        }

        public static OptionDto ToDto(this OptionEntity o, object? value = null)
        {
            return new OptionDto(
                o.Id,
                o.Key,
                value ?? CoreTypesHelper.DeserializeUsingCoreType(o.ValueJson, o.TypeName),
                o.TypeName
                );
        }
    }
}
