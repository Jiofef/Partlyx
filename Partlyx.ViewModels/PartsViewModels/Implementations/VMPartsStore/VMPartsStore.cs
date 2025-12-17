using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Events;
using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public partial class VMPartsStore : IVMPartsStore, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IDisposable _fileClosedSubscription;

        private Dictionary<Guid, ResourceViewModel> _resources { get; }
        private Dictionary<Guid, RecipeViewModel> _recipes { get; }
        private Dictionary<Guid, RecipeComponentViewModel> _components { get; }
        private Dictionary<Guid, List<RecipeComponentViewModel>> _componentsWithResource { get; }

        public IReadOnlyDictionary<Guid, ResourceViewModel> Resources => _resources;
        public IReadOnlyDictionary<Guid, RecipeViewModel> Recipes => _recipes;
        public IReadOnlyDictionary<Guid, RecipeComponentViewModel> Components => _components;
        public IReadOnlyDictionary<Guid, List<RecipeComponentViewModel>> ComponentsWithResource => _componentsWithResource;

        public VMPartsStore(IEventBus bus)
        {
            _bus = bus;

            _resources = new();
            _recipes = new();
            _components = new();
            _componentsWithResource = new();

            _fileClosedSubscription = bus.Subscribe<FileClosedUIEvent>((ev) => ClearStore(), true);
        }

        public void Register(ResourceViewModel resource)
        {
            if (!_resources.ContainsKey(resource.Uid))
            {
                _resources.Add(resource.Uid, resource);
                _bus.Publish(new ResourceVMAddedToStoreEvent(resource.Uid));

                // Complete pending awaiters for this uid
                CompletePendingAwaiters(resource.Uid, resource);
            }
        }

        public void Register(RecipeViewModel recipe)
        {
            if (!_recipes.ContainsKey(recipe.Uid))
            {
                _recipes.Add(recipe.Uid, recipe);
                _bus.Publish(new RecipeVMAddedToStoreEvent(recipe.Uid));

                CompletePendingAwaiters(recipe.Uid, recipe);
            }
        }

        public void Register(RecipeComponentViewModel component)
        {
            if (!_components.ContainsKey(component.Uid))
            {
                var resource = component.Resource;
                if (resource != null)
                {
                    if (!_componentsWithResource.ContainsKey(resource.Uid))
                        _componentsWithResource.Add(resource.Uid, new List<RecipeComponentViewModel>());
                    _componentsWithResource[resource.Uid].Add(component);
                }

                _components.Add(component.Uid, component);
                _bus.Publish(new RecipeComponentVMAddedToStoreEvent(component.Uid));

                CompletePendingAwaiters(component.Uid, component);
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
            if (_components.ContainsKey(uid))
            {
                var component = _components[uid];
                var resource = component.Resource;
                if (resource != null)
                {
                    var list = _componentsWithResource[resource.Uid];
                    list.Remove(component);

                    if (list.Count == 0)
                        _componentsWithResource.Remove(resource.Uid);
                }

                _components[uid].Dispose();
                _components.Remove(uid);

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
            else if (Components.ContainsKey(itemUidNotNull))
            {
                part = Components[itemUidNotNull];
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
            _components.ClearAndDispose();

            // Notify awaiters that nothing will appear (store was cleared).
            CompleteAllPendingWithNull();
        }

        public void Dispose()
        {
            _fileClosedSubscription.Dispose();
            ClearStore();
        }
    }
}
