using Partlyx.Core.VisualsInfo;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IIconInfoProvider
    {
        IconInfo GetInfo(IIcon icon);
    }
}