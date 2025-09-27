using Partlyx.Core;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public class VMPartsStore : IVMPartsStore
    {
        private readonly IEventBus _bus;

        private Dictionary<Guid, ResourceItemViewModel> _resources { get; }
        private Dictionary<Guid, RecipeItemViewModel> _recipes { get; }
        private Dictionary<Guid, RecipeComponentItemViewModel> _recipeComponents { get; }

        public IReadOnlyDictionary<Guid, ResourceItemViewModel> Resources => _resources;
        public IReadOnlyDictionary<Guid, RecipeItemViewModel> Recipes => _recipes;
        public IReadOnlyDictionary<Guid, RecipeComponentItemViewModel> RecipeComponents => _recipeComponents;

        public VMPartsStore(IEventBus bus)
        {
            _bus = bus;

            _resources = new();
            _recipes = new();
            _recipeComponents = new();

            bus.Subscribe<PartsInitializationStartedEvent>((ev) => ClearStore(), true);
            bus.Subscribe<FileClearedEvent>((ev) => ClearStore(), true);
        }

        public void Register(ResourceItemViewModel resource)
        {
            if (!_resources.ContainsKey(resource.Uid))
            {
                _resources.Add(resource.Uid, resource);
                _bus.Publish(new ResourceVMAddedToStoreEvent(resource.Uid));
            }
        }

        public void Register(RecipeItemViewModel recipe)
        {
            if (!_resources.ContainsKey(recipe.Uid))
            {
                _recipes.Add(recipe.Uid, recipe);
                _bus.Publish(new RecipeVMAddedToStoreEvent(recipe.Uid));
            }
        }

        public void Register(RecipeComponentItemViewModel component)
        {
            if (!_resources.ContainsKey(component.Uid))
            {
                _recipeComponents.Add(component.Uid, component);
                _bus.Publish(new RecipeComponentVMAddedToStoreEvent(component.Uid));
            }
        }

        public void RemoveResource(Guid uid)
        {
            if (_resources.Remove(uid))
                _bus.Publish(new ResourceVMRemovedFromStoreEvent(uid));
        }

        public void RemoveRecipe(Guid uid)
        {
            if (_recipes.Remove(uid))
                _bus.Publish(new RecipeVMRemovedFromStoreEvent(uid));
        }

        public void RemoveRecipeComponent(Guid uid)
        {
            if (_recipeComponents.Remove(uid))
                _bus.Publish(new RecipeComponentVMRemovedFromStoreEvent(uid));
        }

        public bool TryGet(Guid itemUid, out IVMPart? part)
        {
            if (Resources.ContainsKey(itemUid))
            {
                part = Resources[itemUid];
                return true;
            }
            else if (Recipes.ContainsKey(itemUid))
            {
                part = Recipes[itemUid];
                return true;
            }
            else if (RecipeComponents.ContainsKey(itemUid))
            {
                part = RecipeComponents[itemUid];
                return true;
            }
            else
            {
                part = null;
                return false;
            }
        }

        private void ClearStore()
        {
            _resources.Clear();
            _recipes.Clear();
            _recipeComponents.Clear();
        }
    }
}
