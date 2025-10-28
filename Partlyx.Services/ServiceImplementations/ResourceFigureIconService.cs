using Partlyx.Core;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;

namespace Partlyx.Services.ServiceImplementations
{
    public class ResourceFigureIconService : FigureIconServiceAbstract, IResourceFigureIconService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public ResourceFigureIconService(IPartlyxRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        protected override async Task TryExcecuteOnFigureIconAsync(Func<FigureIcon, Task> action, params Guid[] parentUids)
        {
            await _repo.ExecuteOnResourceAsync(parentUids[0], async res =>
            {
                var iconInfo = res.GetIconInfo();
                var icon = _infoProvider.GetFigureIconFromInfo(iconInfo);

                if (icon == null) return;
                await action(icon);
                var info = _infoProvider.GetInfo(icon);
                res.UpdateIconInfo(info);

                var ev = new ResourceUpdatedEvent(res.ToDto(), ["Icon"]);
                _eventBus.Publish(ev);
            });
        }
    }

    public class RecipeFigureIconService : FigureIconServiceAbstract, IRecipeFigureIconService
    {
        private readonly IPartlyxRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IIconInfoProvider _infoProvider;
        public RecipeFigureIconService(IPartlyxRepository repo, IEventBus bus, IIconInfoProvider iip)
        {
            _repo = repo;
            _eventBus = bus;
            _infoProvider = iip;
        }

        protected override async Task TryExcecuteOnFigureIconAsync(Func<FigureIcon, Task> action, params Guid[] parentUids)
        {
            await _repo.ExecuteOnRecipeAsync(parentUids[0], parentUids[1], async recipe =>
            {
                var iconInfo = recipe.GetIconInfo();
                var icon = _infoProvider.GetFigureIconFromInfo(iconInfo);

                if (icon == null) return;
                await action(icon);
                var info = _infoProvider.GetInfo(icon);
                recipe.UpdateIconInfo(info);

                var ev = new RecipeUpdatedEvent(recipe.ToDto(), ["Icon"]);
                _eventBus.Publish(ev);
            });
        }
    }
}
