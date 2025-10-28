using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceImageIconService : ImageIconServiceAbstract, IResourceImageIconService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceImageIconService(IPartlyxRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        protected override async Task TryExcecuteOnImageIconAsync(Func<ImageIcon, Task> action, params Guid[] parentUids)
        {
            await _repo.ExecuteOnResourceAsync(parentUids[0], async res =>
            {
                var iconInfo = res.GetIconInfo();
                var icon = _infoProvider.GetImageIconFromInfo(iconInfo);

                if (icon == null) return;
                await action(icon);
                var info = _infoProvider.GetInfo(icon);
                res.UpdateIconInfo(info);

                var ev = new ResourceUpdatedEvent(res.ToDto(), ["Icon"]);
                _eventBus.Publish(ev);
            });
        }
    }
}
