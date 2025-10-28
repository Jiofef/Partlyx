using Partlyx.Core.VisualsInfo;
using Partlyx.Services.ServiceInterfaces;
using System.Text.Json;

namespace Partlyx.Services.ServiceImplementations
{
    public class IconInfoProvider : IIconInfoProvider
    {
        public IconInfoProvider() { }

        public IconInfo GetInfo(IIcon icon)
        {
            IconTypeEnum type;
            if (icon is ImageIcon)
                type = IconTypeEnum.Image;
            else if (icon is FigureIcon)
                type = IconTypeEnum.Figure;
            else
                throw new NotSupportedException();

            string data = JsonSerializer.Serialize(icon);

            return new IconInfo(type, data);
        }

        public ImageIcon? GetImageIconFromInfo(IconInfo info)
        {
            return JsonSerializer.Deserialize<ImageIcon>(info.Data);
        }

        public FigureIcon? GetFigureIconFromInfo(IconInfo info)
        {
            return JsonSerializer.Deserialize<FigureIcon>(info.Data);
        }
    }
}
