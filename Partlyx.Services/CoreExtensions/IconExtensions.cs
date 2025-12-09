using Partlyx.Core.VisualsInfo;
using Partlyx.Services.Dtos;
using Newtonsoft.Json;

namespace Partlyx.Services.CoreExtensions
{
    public static class IconExtensions
    {
        public static IconInfo GetInfo(this IIcon icon)
        {
            IconTypeEnum type;
            if (icon is FigureIcon)
                type = IconTypeEnum.Figure;
            else if (icon is ImageIcon)
                type = IconTypeEnum.Image;
            else if (icon is InheritedIcon)
                type = IconTypeEnum.Inherited;
            else
                throw new NotSupportedException();

            string data = JsonConvert.SerializeObject(icon);

            return new IconInfo(type, data);
        }


        public static IconInfo ToIconInfo(this IconDto dto)
        {
            if (dto is FigureIconDto fidto)
            {
                var icon = new FigureIcon(fidto.Color, fidto.FigureType);
                return icon.GetInfo();
            }
            else if (dto is ImageIconDto iidto)
            {
                var icon = new ImageIcon(iidto.ImageUid);
                return icon.GetInfo();
            }
            else if (dto is InheritedIconDto inidto)
            {
                var icon = new InheritedIcon(inidto.ParentUid, inidto.ParentType);
                return icon.GetInfo();
            }
            else
                return new NullIcon().GetInfo();
        }
    }
}
