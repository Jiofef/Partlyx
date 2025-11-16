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

        private readonly IDisposable _resourceRemoveSubscription;
        private readonly IDisposable _recipeRemoveSubscription;
        private readonly IDisposable _componentRemoveSubscription;

        public VMPartsStoreCleaner(IVMPartsStore store, IEventBus bus)
        {
            _store = store;

            _resourceRemoveSubscription = bus.Subscribe<ResourceDeletedEvent>(ev => RemoveResource(ev.ResourceUid));
            _recipeRemoveSubscription = bus.Subscribe<RecipeDeletedEvent>(ev => RemoveRecipe(ev.RecipeUid));
            _componentRemoveSubscription = bus.Subscribe<RecipeComponentDeletedEvent>(ev => RemoveComponent(ev.RecipeComponentUid));
        }

        private void RemoveResource(Guid uid)
        {
            var resource = _store.Resources.GetValueOrDefault(uid);
            if (resource == null) return;

            _store.RemoveResource(uid);
        }
        private void RemoveRecipe(Guid uid)
        {
            var recipe = _store.Recipes.GetValueOrDefault(uid);
            if (recipe == null) return;

            _store.RemoveRecipe(uid);
        }
        private void RemoveComponent(Guid uid)
        {
            _store.RemoveRecipeComponent(uid);
        }

        public void Dispose()
        {
            _resourceRemoveSubscription.Dispose();
            _recipeRemoveSubscription.Dispose();
            _componentRemoveSubscription.Dispose();
        }
    }
}
