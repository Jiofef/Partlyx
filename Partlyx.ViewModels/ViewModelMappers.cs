using Partlyx.Services.Dtos;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;

namespace Partlyx.ViewModels
{
    public static class ViewModelMappers
    {
        public static IconDto ToDto(this IconViewModel iconVM)
        {
            switch(iconVM.IconType)
            {
                case IconTypeEnumViewModel.Null:
                    return new NullIconDto();
                case IconTypeEnumViewModel.Vector:
                    if (iconVM.Content is not IconVectorContentViewModel iconFigureContent)
                        return new NullIconDto();

                    var iconDto = new FigureIconDto(iconFigureContent.FigureColor, iconFigureContent.FigureType);
                    return iconDto;
                case IconTypeEnumViewModel.Image:
                    if (iconVM.Content is not ImageViewModel imageContent)
                        return new NullIconDto();

                    var imageDto = new ImageIconDto(imageContent.Uid);
                    return imageDto;
                case IconTypeEnumViewModel.Inherited:
                    if (iconVM.Content is not InheritedIconContentViewModel inheritedContent)
                        return new NullIconDto();

                    var inheritedIcon = new InheritedIconDto(inheritedContent.ParentUid, inheritedContent.ParentType);
                    return inheritedIcon;
                default:
                    return new NullIconDto();
            }
        }

        public static ResourceDto ToDto(this ResourceViewModel resourceVM)
            => new ResourceDto(
                resourceVM.Uid,
                resourceVM.Name,
                resourceVM.LinkedDefaultRecipe?.Uid,
                resourceVM.Icon.ToDto());

        public static RecipeDto ToDto(this RecipeViewModel recipeVM)
            => new RecipeDto(
                recipeVM.Uid,
                recipeVM.Name,
                recipeVM.IsReversible,
                recipeVM.Inputs.Select(c => c.ToDto()).ToList(),
                recipeVM.Outputs.Select(c => c.ToDto()).ToList(),
                recipeVM.Icon.ToDto());

        public static RecipeComponentDto ToDto(this RecipeComponentViewModel componentVM)
            => new RecipeComponentDto(
                componentVM.Uid,
                componentVM.LinkedParentRecipe?.Uid,
                componentVM.Resource.Uid,
                componentVM.Quantity,
                componentVM.IsOutput,
                componentVM.LinkedSelectedRecipe?.Uid);
    }
}
