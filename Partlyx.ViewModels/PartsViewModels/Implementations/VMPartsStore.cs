using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class VMPartsStore : IVMPartsStore
    {
        public Dictionary<Guid, ResourceItemViewModel> Resources { get; }
        public Dictionary<Guid, RecipeItemViewModel> Recipes { get; }
        public Dictionary<Guid, RecipeComponentItemViewModel> RecipeComponents { get; }

        public VMPartsStore(IEventBus bus)
        {
            Resources = new();
            Recipes = new();
            RecipeComponents = new();

            bus.Subscribe<PartsInitializationStartedEvent>((ev) => ClearStore(), true);
            bus.Subscribe<FileClearedEvent>((ev) => ClearStore(), true);
        }

        public void Register(ResourceItemViewModel resource)
        {
            Resources.Add(resource.Uid, resource);
        }

        public void Register(RecipeItemViewModel recipe)
        {
            Recipes.Add(recipe.Uid, recipe);
        }

        public void Register(RecipeComponentItemViewModel component)
        {
            RecipeComponents.Add(component.Uid, component);
        }

        private void ClearStore()
        {
            Resources.Clear();
            Recipes.Clear();
            RecipeComponents.Clear();
        }
    }
}
