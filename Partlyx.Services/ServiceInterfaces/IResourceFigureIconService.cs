using System.Drawing;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IResourceFigureIconService
    {
        Task SetColorAsync(Guid parentResourceUid, Color color);
    }
}