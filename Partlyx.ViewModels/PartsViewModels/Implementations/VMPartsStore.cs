using Partlyx.Core;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Events;
using Partlyx.Services.PartsEventClasses;
using Partlyx.UI.Avalonia.Helpers;
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
            if (!_recipes.ContainsKey(recipe.Uid))
            {
                _recipes.Add(recipe.Uid, recipe);
                _bus.Publish(new RecipeVMAddedToStoreEvent(recipe.Uid));
            }
        }

        public void Register(RecipeComponentViewModel component)
        {
            if (!_recipeComponents.ContainsKey(component.Uid))
            {
                _recipeComponents.Add(component.Uid, component);
                _bus.Publish(new RecipeComponentVMAddedToStoreEvent(component.Uid));
            }
        }

        public void RemoveResource(Guid uid)
        {
            if (_resources.ContainsKey(uid))
            {
                _resources[uid].Dispose();
                _resources.Remove(uid);
                _bus.Publish(new ResourceVMRemovedFromStoreEvent(uid));
            }
        }

        public void RemoveRecipe(Guid uid)
        {
            if (_recipes.ContainsKey(uid))
            {
                _recipes[uid].Dispose();
                _recipes.Remove(uid);
                _bus.Publish(new RecipeVMRemovedFromStoreEvent(uid));
            }
        }

        public void RemoveRecipeComponent(Guid uid)
        {
            if (_recipeComponents.ContainsKey(uid))
            {
                _recipeComponents[uid].Dispose();
                _recipeComponents.Remove(uid);
                _bus.Publish(new RecipeComponentVMRemovedFromStoreEvent(uid));
            }
        }

        public bool TryGet(Guid? itemUid, out IVMPart? part)
        {
            if (itemUid == null)
            {
                part = null;
                return true;
            }

            Guid itemUidNotNull = (Guid)itemUid;
            if (Resources.ContainsKey(itemUidNotNull))
            {
                part = Resources[itemUidNotNull];
                return true;
            }
            else if (Recipes.ContainsKey(itemUidNotNull))
            {
                part = Recipes[itemUidNotNull];
                return true;
            }
            else if (RecipeComponents.ContainsKey(itemUidNotNull))
            {
                part = RecipeComponents[itemUidNotNull];
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
            _resources.ClearAndDispose();
            _recipes.ClearAndDispose();
            _recipeComponents.ClearAndDispose();
        }

        public void Dispose()
        {
            _fileClosedSubscription.Dispose();
            ClearStore();
        }
    }
}
