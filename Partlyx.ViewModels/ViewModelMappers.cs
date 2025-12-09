using Partlyx.Services.Dtos;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;

namespace Partlyx.ViewModels
{
    public static class ViewModelMappers
    {
        public static IconDto ToDto(this IconViewModel iconVM)
        {
            switch(iconVM.IconType)
            {
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
    }
}
