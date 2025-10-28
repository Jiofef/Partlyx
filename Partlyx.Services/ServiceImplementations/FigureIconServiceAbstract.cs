using Partlyx.Core.VisualsInfo;
using System.Drawing;

namespace Partlyx.Services.ServiceImplementations
{
    public abstract class FigureIconServiceAbstract
    {
        public async Task SetFigureColorAsync(Color color, params Guid[] parentUids)
        {
            await TryExcecuteOnFigureIconAsync(icon =>
            {
                icon.Color = color;
                return Task.CompletedTask;
            },
            parentUids);
        }

        /// <summary>
        /// You can see the valid types through FigureTypes in Partlyx.Services.Dtos namespace
        /// </summary>
        public async Task SetFigureTypeAsync(string figureType, params Guid[] parentUids)
        {
            await TryExcecuteOnFigureIconAsync(icon =>
            {
                icon.FigureType = figureType;
                return Task.CompletedTask;
            },
            parentUids);
        }

        public async Task<FigureIcon?> GetFigureIconAsync(params Guid[] parentUids)
        {
            FigureIcon? result = null;
            await TryExcecuteOnFigureIconAsync(icon =>
            {
                result = icon;
                return Task.CompletedTask;
            },
            parentUids);
            return result;
        }


        /// <summary>
        /// Should automate getting icon object and saving changes to changed entity and DB.
        /// </summary>
        protected abstract Task TryExcecuteOnFigureIconAsync(Func<FigureIcon, Task> action, params Guid[] parentUids);
    }
}
