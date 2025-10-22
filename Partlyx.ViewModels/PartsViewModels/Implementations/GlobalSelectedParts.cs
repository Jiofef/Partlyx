using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.Specialized;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class GlobalSelectedParts : SelectedPartsAbstract, IGlobalSelectedParts, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly List<IDisposable> _subscriptions = new();
        public GlobalSelectedParts(IEventBus bus)
        {
            _bus = bus;

            var fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => ClearSelection());
            _subscriptions.Add(fileClosedSubscription);
        }

        protected override void SelectedResourcesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleResourceSelected)
            {
                var @event = new GlobalSingleResourceSelectedEvent(GetSingleResourceOrNull()!.Uid);
                _bus.Publish(@event);

                var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Resource, GetSingleResourceOrNull()!.Uid);
                _bus.Publish(@commonEvent);
            }
            else if (Resources.Count > 1 && args.NewItems != null) // Multiselect
            {
                foreach (ResourceViewModel added in args.NewItems)
                {
                    var @event = new GlobalResourceAddedToSelectedEvent(added.Uid);
                    _bus.Publish(@event);

                    var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Resource, added.Uid);
                    _bus.Publish(@commonEvent);
                }
            }
            else
            {
                var @event = new GlobalSingleResourceSelectedEvent(null);
                _bus.Publish(@event);
            }

            var selectedResourcesUids = Resources.Select(r => r.Uid).ToArray();
            var @changedEvent = new GlobalSelectedResourcesChangedEvent(selectedResourcesUids);
            _bus.Publish(@changedEvent);

            var @commonChangedEvent = new GlobalPartsSelectedChangedEvent(PartTypeEnumVM.Resource, selectedResourcesUids);
            _bus.Publish(@commonChangedEvent);
        }

        protected override void SelectedRecipesChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleRecipeSelected)
            {
                var @event = new GlobalSingleRecipeSelectedEvent(GetSingleRecipeOrNull()!.Uid);
                _bus.Publish(@event);

                var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Recipe, GetSingleRecipeOrNull()!.Uid);
                _bus.Publish(@commonEvent);
            }
            else if (Recipes.Count > 1 && args.NewItems != null) // Multiselect
            {
                foreach (RecipeViewModel added in args.NewItems)
                {
                    var @event = new GlobalRecipeAddedToSelectedEvent(added.Uid);
                    _bus.Publish(@event);

                    var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Recipe, added.Uid);
                    _bus.Publish(@commonEvent);
                }
            }
            else
            {
                var @event = new GlobalSingleRecipeSelectedEvent(null);
                _bus.Publish(@event);
            }

                var selectedRecipeUids = Recipes.Select(r => r.Uid).ToArray();
            var @changedEvent = new GlobalSelectedRecipesChangedEvent(selectedRecipeUids);
            _bus.Publish(@changedEvent);

            var @commonChangedEvent = new GlobalPartsSelectedChangedEvent(PartTypeEnumVM.Recipe, selectedRecipeUids);
            _bus.Publish(@commonChangedEvent);
        }

        protected override void SelectedComponentsChangedHandler(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (IsSingleComponentSelected)
            {
                var @event = new GlobalSingleComponentSelectedEvent(GetSingleComponentOrNull()!.Uid);
                _bus.Publish(@event);

                var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Component, GetSingleComponentOrNull()!.Uid);
                _bus.Publish(@commonEvent);
            }
            else if (Components.Count > 1 && args.NewItems != null) // Multiselect
            {
                foreach (RecipeComponentViewModel added in args.NewItems)
                {
                    var @event = new GlobalComponentAddedToSelectedEvent(added.Uid);
                    _bus.Publish(@event);

                    var @commonEvent = new GlobalSinglePartSelectedEvent(PartTypeEnumVM.Component, added.Uid);
                    _bus.Publish(@commonEvent);
                }
            }
            else
            {
                var @event = new GlobalSingleComponentSelectedEvent(null);
                _bus.Publish(@event);
            }

            var selectedComponentsUids = Components.Select(c => c.Uid).ToArray();
            var @changedEvent = new GlobalSelectedComponentsChangedEvent(selectedComponentsUids);
            _bus.Publish(@changedEvent);

            var @commonChangedEvent = new GlobalPartsSelectedChangedEvent(PartTypeEnumVM.Component, selectedComponentsUids);
            _bus.Publish(@commonChangedEvent);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
