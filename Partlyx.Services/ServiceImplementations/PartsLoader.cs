using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.Services.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Services.ServiceImplementations
{
    public class PartsLoader : IPartsLoader
    {
        private readonly IEventBus _bus;
        private readonly IResourceRepository _repo;
        public PartsLoader(IEventBus bus, IResourceRepository repo)
        {
            _bus = bus;
            _repo = repo;

            _bus.SubscribeAsync<PartlyxDBInitializedEvent>(OnDBInitialized);
        }

        private async Task OnDBInitialized(PartlyxDBInitializedEvent ev)
        {
            _bus.Publish(new PartsInitializationStartedEvent());

            var resources = await _repo.GetAllTheResourcesAsync();
            var rDtos = resources.Select(x => x.ToDto()).ToArray();
            _bus.Publish(new ResourcesBulkLoadedEvent(rDtos));

            var recipes = await _repo.GetAllTheRecipesAsync();
            var r2Dtos = recipes.Select(x => x.ToDto()).ToArray();
            _bus.Publish(new RecipesBulkLoadedEvent(r2Dtos));

            var components = await _repo.GetAllTheRecipeComponentsAsync();
            var cDtos = components.Select(x => x.ToDto()).ToArray();
            _bus.Publish(new RecipeComponentsBulkLoadedEvent(cDtos));

            _bus.Publish(new PartsInitializationFinishedEvent());
        }
    }
}
