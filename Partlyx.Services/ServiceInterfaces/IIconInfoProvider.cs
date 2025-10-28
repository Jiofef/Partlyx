using Partlyx.Core.VisualsInfo;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IIconInfoProvider
    {
        FigureIcon? GetFigureIconFromInfo(IconInfo info);
        ImageIcon? GetImageIconFromInfo(IconInfo info);
        IconInfo GetInfo(IIcon icon);
    }
}