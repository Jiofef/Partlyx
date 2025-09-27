using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public class LinkedPartsManager : IDisposable, ILinkedPartsManager
    {
        private readonly IDispatcherInvoker _invoker;
        private readonly IGuidLinkedPartFactory _factory;
        private readonly IEventBus _bus;
        private readonly IVMPartsStore _store;
        public LinkedPartsManager(IDispatcherInvoker invoker, IGuidLinkedPartFactory factory, IEventBus bus, IVMPartsStore store)
        {
            _invoker = invoker;
            _factory = factory;
            _bus = bus;
            _store = store;

            _bus.Subscribe<ResourceVMAddedToStoreEvent>((ev) => NotifyAdded(ev.resourceUid, _store.Resources[ev.resourceUid], _linkedResourcesDic));
            _bus.Subscribe<RecipeVMAddedToStoreEvent>((ev) => NotifyAdded(ev.recipeUid, _store.Recipes[ev.recipeUid], _linkedRecipesDic));
            _bus.Subscribe<RecipeComponentVMAddedToStoreEvent>((ev) => NotifyAdded(ev.componentUid, _store.RecipeComponents[ev.componentUid], _linkedComponentsDic));

            _bus.Subscribe<ResourceVMRemovedFromStoreEvent>((ev) => NotifyRemoved(ev.resourceUid, _linkedResourcesDic));
            _bus.Subscribe<RecipeVMRemovedFromStoreEvent>((ev) => NotifyRemoved(ev.recipeUid, _linkedResourcesDic));
            _bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>((ev) => NotifyRemoved(ev.componentUid, _linkedResourcesDic));
        }

        private List<WeakReference<GuidLinkedPart<ResourceItemViewModel>>> _linkedResourcesDisorderedList = new();
        private List<WeakReference<GuidLinkedPart<RecipeItemViewModel>>> _linkedRecipesDisorderedList = new();
        private List<WeakReference<GuidLinkedPart<RecipeComponentItemViewModel>>> _linkedRecipeComponentsDisorderedList = new();

        private ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<ResourceItemViewModel>>>> _linkedResourcesDic = new();
        private ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<RecipeItemViewModel>>>> _linkedRecipesDic = new();
        private ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<RecipeComponentItemViewModel>>>> _linkedComponentsDic = new();

        public GuidLinkedPart<ResourceItemViewModel> CreateAndRegisterLinkedResourceVM(Guid uid)
            => CreateAndRegisterLinkedPart<ResourceItemViewModel>(uid);

        public GuidLinkedPart<RecipeItemViewModel> CreateAndRegisterLinkedRecipeVM(Guid uid)
            => CreateAndRegisterLinkedPart<RecipeItemViewModel>(uid);

        public GuidLinkedPart<RecipeComponentItemViewModel> CreateAndRegisterLinkedRecipeComponentVM(Guid uid)
            => CreateAndRegisterLinkedPart<RecipeComponentItemViewModel>(uid);

        private GuidLinkedPart<TPart> CreateAndRegisterLinkedPart<TPart>(Guid uid) where TPart : IVMPart
        {
            var linkedPart = _factory.CreateLinkedPart<TPart>(uid);

            if (linkedPart is GuidLinkedPart<ResourceItemViewModel> rl)
                Register(rl);
            else if (linkedPart is GuidLinkedPart<RecipeItemViewModel> rcl)
                Register(rcl);
            else if (linkedPart is GuidLinkedPart<RecipeComponentItemViewModel> cl)
                Register(cl);

            return linkedPart;
        }

        public void Register(GuidLinkedPart<ResourceItemViewModel> linkedPart)
            => Register(linkedPart, _linkedResourcesDic, _linkedResourcesDisorderedList);

        public void Register(GuidLinkedPart<RecipeItemViewModel> linkedPart)
            => Register(linkedPart, _linkedRecipesDic, _linkedRecipesDisorderedList);

        public void Register(GuidLinkedPart<RecipeComponentItemViewModel> linkedPart)
            => Register(linkedPart, _linkedComponentsDic, _linkedRecipeComponentsDisorderedList);

        private void Register<TPart>(
            GuidLinkedPart<TPart> linkedPart,
            ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<TPart>>>> dic,
            List<WeakReference<GuidLinkedPart<TPart>>> disorderedList)
            where TPart : IVMPart
        {
            var weak = new WeakReference<GuidLinkedPart<TPart>>(linkedPart);

            lock (disorderedList)
            {
                if (!disorderedList.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, linkedPart)))
                    disorderedList.Add(weak);
            }

            var uid = linkedPart.Uid;
            var list = dic.GetOrAdd(uid, _ => new List<WeakReference<GuidLinkedPart<TPart>>>());
            lock (list)
            {
                if (!list.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, linkedPart)))
                    list.Add(weak);

                list.RemoveAll(wr => !wr.TryGetTarget(out _));
                if (list.Count == 0) dic.TryRemove(uid, out _);
            }

            linkedPart.Disposed += () => Unregister(linkedPart, dic, disorderedList);

            PushCurrentValueFor(linkedPart, uid);
        }

        private void PushCurrentValueFor<TPart>(GuidLinkedPart<TPart> linkedPart, Guid uid)
            where TPart : IVMPart
        {
            if (_store.TryGet(uid, out IVMPart? found))
            {
                DispatchToUi(() => linkedPart.Value = (TPart)found!);
            }
            else
            {
                DispatchToUi(() => linkedPart.Value = default);
            }
        }

        private void Unregister<TPart>(
            GuidLinkedPart<TPart> linkedPart,
            ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<TPart>>>> dic,
            List<WeakReference<GuidLinkedPart<TPart>>> disorderedList)
            where TPart : IVMPart
        {
            lock (disorderedList)
            {
                disorderedList.RemoveAll(wr => !wr.TryGetTarget(out var t) || ReferenceEquals(t, linkedPart));
            }

            foreach (var kv in dic.ToArray())
            {
                var list = kv.Value;
                lock (list)
                {
                    list.RemoveAll(wr => !wr.TryGetTarget(out var t) || ReferenceEquals(t, linkedPart));
                    if (list.Count == 0) dic.TryRemove(kv.Key, out _);
                }
            }
        }

        private void NotifyAdded<TPart>(Guid uid, TPart instance, ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<TPart>>>> dic)
            where TPart : IVMPart
        {
            if (!dic.TryGetValue(uid, out var list)) return;

            List<GuidLinkedPart<TPart>> targets;
            lock (list)
            {
                list.RemoveAll(wr => !wr.TryGetTarget(out _));
                targets = list.Select(wr => { wr.TryGetTarget(out var t); return t; }).Where(x => x != null).ToList()!;
            }

            foreach (var t in targets)
            {
                DispatchToUi(() => t.Value = instance);
            }
        }

        private void NotifyRemoved<TPart>(Guid uid, ConcurrentDictionary<Guid, List<WeakReference<GuidLinkedPart<TPart>>>> dic)
            where TPart : IVMPart
        {
            if (!dic.TryGetValue(uid, out var list)) return;

            List<GuidLinkedPart<TPart>> targets;
            lock (list)
            {
                list.RemoveAll(wr => !wr.TryGetTarget(out _));
                targets = list.Select(wr => { wr.TryGetTarget(out var t); return t; }).Where(x => x != null).ToList()!;
            }

            foreach (var t in targets)
            {
                DispatchToUi(() => t.Value = default);
            }
        }

        private void DispatchToUi(Action action)
        {
            if (_invoker != null)
            {
                if (_invoker.CheckAccess()) _invoker.Invoke(action);
                else _invoker.BeginInvoke(action);
            }
            else
            {
                action(); // for tests or no ui
            }
        }


        public void Dispose()
        {
            //
        }
    }
}
