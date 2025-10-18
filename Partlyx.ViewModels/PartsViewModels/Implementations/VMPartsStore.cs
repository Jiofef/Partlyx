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
    public class VMPartsStore : IVMPartsStore, IDisposable
    {
        private readonly IEventBus _bus;

        private readonly IDisposable _fileClosedSubscription;

        private Dictionary<Guid, ResourceViewModel> _resources { get; }
        private Dictionary<Guid, RecipeViewModel> _recipes { get; }
        private Dictionary<Guid, RecipeComponentViewModel> _recipeComponents { get; }

        public IReadOnlyDictionary<Guid, ResourceViewModel> Resources => _resources;
        public IReadOnlyDictionary<Guid, RecipeViewModel> Recipes => _recipes;
        public IReadOnlyDictionary<Guid, RecipeComponentViewModel> RecipeComponents => _recipeComponents;

        public VMPartsStore(IEventBus bus)
        {
            _bus = bus;

            _resources = new();
            _recipes = new();
            _recipeComponents = new();

            _fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => ClearStore(), true);
        }

        public void Register(ResourceViewModel resource)
        {
            if (!_resources.ContainsKey(resource.Uid))
            {
                _resources.Add(resource.Uid, resource);
                _bus.Publish(new ResourceVMAddedToStoreEvent(resource.Uid));
            }
        }

        public void Register(RecipeViewModel recipe)
        {
            if (!_resources.ContainsKey(recipe.Uid))
            {
                _recipes.Add(recipe.Uid, recipe);
                _bus.Publish(new RecipeVMAddedToStoreEvent(recipe.Uid));
            }
        }

        public void Register(RecipeComponentViewModel component)
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

        public void Dispose()
        {
            _fileClosedSubscription.Dispose();
        }
    }
}
