using Partlyx.Core.VisualsInfo;
using System.Drawing;

namespace Partlyx.Services.ServiceInterfaces
{
    public interface IFigureIconService
    {
        Task SetFigureColorAsync(Color color, params Guid[] parentUids);
        Task SetFigureTypeAsync(string figureType, params Guid[] parentUids);

        Task<FigureIcon?> GetFigureIconAsync(params Guid[] parentUids);
    }
    public interface IImageIconService
    {
        Task SetImageUidAsync(Guid uid, params Guid[] parentUids);

        Task<ImageIcon?> GetImageIconAsync(params Guid[] parentUids);
    }
}
