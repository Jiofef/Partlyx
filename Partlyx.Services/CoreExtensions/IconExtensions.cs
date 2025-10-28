using Partlyx.Core.VisualsInfo;
using System.Text.Json;

namespace Partlyx.Services.CoreExtensions
{
    public static class IconExtensions
    {
        public static IconInfo GetInfo(this IIcon icon)
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
    }
}
