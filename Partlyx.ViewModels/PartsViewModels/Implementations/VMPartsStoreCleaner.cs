using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class VMPartsStoreCleaner : IVMPartsStoreCleaner
    {
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;

        private readonly IDisposable _resourceRemoveSubscription;
        private readonly IDisposable _recipeRemoveSubscription;
        private readonly IDisposable _componentRemoveSubscription;

        public VMPartsStoreCleaner(IVMPartsStore store, IEventBus bus)
        {
            _store = store;
            _bus = bus;

            _resourceRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(ev => RemoveResource(ev.ResourceUid));
            _recipeRemoveSubscription = bus.Subscribe<RecipeDeletedEvent>(ev => RemoveResource(ev.RecipeUid));
            _componentRemoveSubscription = bus.Subscribe<RecipeComponentDeletedEvent>(ev => RemoveResource(ev.RecipeComponentUid));
        }

        private void RemoveResource(Guid uid)
        {
            var resource = _store.Resources.GetValueOrDefault(uid);
            if (resource == null) return;

            foreach (var recipe in resource.Recipes)
                RemoveRecipe(recipe.Uid);

            _store.Resources.Remove(uid);
        }
        private void RemoveRecipe(Guid uid)
        {
            var recipe = _store.Recipes.GetValueOrDefault(uid);
            if (recipe == null) return;

            foreach (var component in recipe.Components)
                RemoveComponent(component.Uid);

            _store.Recipes.Remove(uid);
        }
        private void RemoveComponent(Guid uid)
        {
            _store.RecipeComponents.Remove(uid);
        }

        public void Dispose()
        {
            _resourceRemoveSubscription.Dispose();
            _recipeRemoveSubscription.Dispose();
            _componentRemoveSubscription.Dispose();
        }
    }
}
