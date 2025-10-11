using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceInterfaces;
using System.Drawing;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceFigureIconService : IResourceFigureIconService
    {
        private readonly IPartsRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceFigureIconService(IPartsRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        public async Task SetColorAsync(Guid parentResourceUid, Color color)
        {
            await TryExcecuteOnFigureIconAsync(parentResourceUid, icon =>
            {
                icon.Color = color;
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// You can see the valid types through FigureTypes in Partlyx.Services.Dtos namespace
        /// </summary>
        public async Task SetFigureTypeAsync(Guid parentResourceUid, string figureType)
        {
            await TryExcecuteOnFigureIconAsync(parentResourceUid, icon =>
            {
                icon.FigureType = figureType;
                return Task.CompletedTask;
            });
        }

        private record FigureIconResourcePair(FigureIcon Figure, Resource Resource);

        /// <summary>
        /// Automates getting icon object and saving changes to Resource and DB. If the icon is null, does not accomplish the task.
        /// </summary>
        private async Task TryExcecuteOnFigureIconAsync(Guid parentResourceUid, Func<FigureIcon, Task> action)
        {
            await _repo.ExecuteOnResourceAsync(parentResourceUid, async res =>
            {
                var icon = res.Icon as FigureIcon;
                if (icon == null) return;
                await action(icon);
                var info = _infoProvider.GetInfo(icon);
                res.UpdateIconInfo(info);
            });
        }
    }
}
