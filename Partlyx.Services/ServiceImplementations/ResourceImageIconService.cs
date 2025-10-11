using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceImageIconService : IResourceImageIconService
    {
        private readonly IPartsRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceImageIconService(IPartsRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        public async Task SetImagePathAsync(Guid parentResourceUid, string path)
        {
            await TryExcecuteOnImageIconAsync(parentResourceUid, icon =>
            {
                icon.Path = path;
                return Task.CompletedTask;
            });
        }

        private async Task TryExcecuteOnImageIconAsync(Guid parentResourceUid, Func<ImageIcon, Task> action)
        {
            await _repo.ExecuteOnResourceAsync(parentResourceUid, async res =>
            {
                var icon = res.Icon as ImageIcon;
                if (icon == null) return;
                await action(icon);
                var info = _infoProvider.GetInfo(icon);
                res.UpdateIconInfo(info);
            });
        }
    }
}
