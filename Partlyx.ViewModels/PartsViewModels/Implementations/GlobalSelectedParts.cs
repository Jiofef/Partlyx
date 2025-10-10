using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.Specialized;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class GlobalSelectedParts : SelectedPartsAbstract, IGlobalSelectedParts
    {
        private readonly IEventBus _bus;
        public GlobalSelectedParts(IEventBus bus)
        {
            _bus = bus;
        }

        protected override void SelectedResourcesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleResourceSelected)
            {
                var @event = new GlobalSingleResourceSelectedEvent(GetSingleResourceOrNull()!.Uid);
                _bus.Publish(@event);
            }
            else if (Resources.Count > 1) // Multiselect
            {
                var addedSelected = (ResourceItemViewModel)sender!;
                var @event = new GlobalResourceAddedToSelectedEvent(addedSelected.Uid);
                _bus.Publish(@event);
            }

            var selectedResourcesUids = Resources.Select(r => r.Uid).ToArray();
            var @changedEvent = new GlobalSelectedResourcesChangedEvent(selectedResourcesUids);
            _bus.Publish(@changedEvent);
        }

        protected override void SelectedRecipesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleRecipeSelected)
            {
                var @event = new GlobalSingleRecipeSelectedEvent(GetSingleRecipeOrNull()!.Uid);
                _bus.Publish(@event);
            }
            else if (Recipes.Count > 1 && args.NewItems != null) // Multiselect
            {
                foreach (RecipeItemViewModel added in args.NewItems)
                {
                    var @event = new GlobalRecipeAddedToSelectedEvent(added.Uid);
                    _bus.Publish(@event);
                }
            }

            var selectedRecipeUids = Recipes.Select(r => r.Uid).ToArray();
            var @changedEvent = new GlobalSelectedRecipesChangedEvent(selectedRecipeUids);
            _bus.Publish(@changedEvent);
        }

        protected override void SelectedComponentsChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleComponentSelected)
            {
                var @event = new GlobalSingleComponentSelectedEvent(GetSingleComponentOrNull()!.Uid);
                _bus.Publish(@event);
            }
            else if (Components.Count > 1 && args.NewItems != null) // Multiselect
            {
                foreach (RecipeComponentItemViewModel added in args.NewItems)
                {
                    var @event = new GlobalComponentAddedToSelectedEvent(added.Uid);
                    _bus.Publish(@event);
                }
            }

            var selectedComponentsUids = Components.Select(c => c.Uid).ToArray();
            var @changedEvent = new GlobalSelectedComponentsChangedEvent(selectedComponentsUids);
            _bus.Publish(@changedEvent);
        }
    }
}
