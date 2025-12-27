using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class ViewModelStorePartsEventRouter : PartlyxObservable, IViewModelStorePartsEventRouter
    {
        private readonly IVMPartsStore _store;
        private readonly IEventBus _bus;
        public ViewModelStorePartsEventRouter(IVMPartsStore store, IEventBus bus)
        {
            _store = store;
            _bus = bus;

            SubscribeToResourceEvents();
            SubscribeToRecipeEvents();
            SubscribeToComponentEvents();
        }

        private void SendToResource(Guid resourceUid, object @event)
        {
            var resource = _store.Resources.GetValueOrDefault(resourceUid);
            if (resource == null) return;

            resource.HandleEvent(@event);
        }
        private void SendToRecipe(Guid recipeUid, object @event)
        {
            var recipe = _store.Recipes.GetValueOrDefault(recipeUid);
            if (recipe == null) return;

            recipe.HandleEvent(@event);
        }
        private void SendToComponent(Guid componentUid, object @event)
        {
            var recipe = _store.Components.GetValueOrDefault(componentUid);
            if (recipe == null) return;

            recipe.HandleEvent(@event);
        }

        private void SubscribeToResourceEvents()
        {
            Disposables.Add(_bus.Subscribe<ResourceUpdatedEvent>(OnResourceUpdated, true));
        }
        private void SubscribeToRecipeEvents()
        {
            Disposables.Add(_bus.Subscribe<RecipeUpdatedEvent>(OnRecipeUpdated, true));
            Disposables.Add(_bus.Subscribe<RecipeComponentCreatedEvent>(OnComponentCreated, true));
            Disposables.Add(_bus.Subscribe<RecipeComponentDeletingStartedEvent>(OnComponentDeletingStarted, true));
            Disposables.Add(_bus.Subscribe<RecipeComponentMovedEvent>(OnComponentMoved, true));
            Disposables.Add(_bus.Subscribe<RecipeComponentUpdatedEvent>(OnComponentUpdated, true));
        }
        private void SubscribeToComponentEvents()
        {
            // Already added by SubscribeToRecipeEvents
            // Disposables.Add(_bus.Subscribe<RecipeComponentUpdatedEvent>(OnComponentUpdated, true));
        }

        private void OnResourceUpdated(ResourceUpdatedEvent resourceUpdated)
        {
            SendToResource(resourceUpdated.Resource.Uid, resourceUpdated);
        }
        private void OnRecipeUpdated(RecipeUpdatedEvent recipeUpdated)
        {
            SendToRecipe(recipeUpdated.Recipe.Uid, recipeUpdated);
        }
        private void OnComponentCreated(RecipeComponentCreatedEvent recipeComponentCreated)
        {
            if (recipeComponentCreated.RecipeComponent.ParentRecipeUid != null)
                SendToRecipe((Guid)recipeComponentCreated.RecipeComponent.ParentRecipeUid, recipeComponentCreated);
        }
        private void OnComponentDeletingStarted(RecipeComponentDeletingStartedEvent recipeComponentDeleted)
        {
            SendToRecipe(recipeComponentDeleted.ParentRecipeUid, recipeComponentDeleted);
        }
        private void OnComponentMoved(RecipeComponentMovedEvent recipeComponentMoved)
        {
            SendToRecipe(recipeComponentMoved.OldRecipeUid, recipeComponentMoved);
            SendToRecipe(recipeComponentMoved.NewRecipeUid, recipeComponentMoved);
        }

        private void OnComponentUpdated(RecipeComponentUpdatedEvent recipeComponentUpdated)
        {
            SendToComponent(recipeComponentUpdated.RecipeComponent.Uid, recipeComponentUpdated);

            var parentRecipeUid = recipeComponentUpdated.RecipeComponent.ParentRecipeUid;
            if (parentRecipeUid != null)
                SendToRecipe(parentRecipeUid.Value, recipeComponentUpdated);
        }
    }
}
