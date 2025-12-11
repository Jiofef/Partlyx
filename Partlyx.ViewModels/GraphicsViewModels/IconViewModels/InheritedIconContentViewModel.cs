using Partlyx.Core.VisualsInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public partial class InheritedIconContentViewModel : PartlyxObservable, IIconContentViewModel, IDisposable
    {
        private readonly InheritedIconHelperServiceViewModel _service;

        public IconTypeEnumViewModel ContentIconType => IconTypeEnumViewModel.Inherited;

        // Cached IsEmpty value to avoid repeated full-chain evaluation on every UI binding request.
        private bool _isEmpty = true;
        public bool IsEmpty { get => _isEmpty; private set => SetProperty(ref _isEmpty, value); }

        public bool IsIdentical(IIconContentViewModel other)
        {
            if (other is not InheritedIconContentViewModel otherInherited) return false;

            if (otherInherited == this) return true;

            return ParentUid == otherInherited.ParentUid;
        }

        public InheritedIconContentViewModel(InheritedIconHelperServiceViewModel service)
        {
            _service = service;
        }
        private Guid _parentUid;
        public Guid ParentUid { get => _parentUid; set => SetProperty(ref _parentUid, value); }

        public InheritedIcon.InheritedIconParentTypeEnum ParentType;

        private IObservableFindableIconHolder? _bindedParent;
        public IObservableFindableIconHolder? BindedParent
        {
            get => _bindedParent;
            private set => SetProperty(ref _bindedParent, value);
        }

        // Direct content inherited from the parent object.
        private IIconContentViewModel? _inhertiedContent;

        /// <summary>
        /// Returns the deepest non-inherited content in the chain.
        /// If the chain contains a cycle or is empty, returns null.
        /// </summary>
        public IIconContentViewModel? InheritedContent
        {
            get
            {
                var current = _inhertiedContent;
                var visited = new HashSet<IIconContentViewModel>();

                while (current is InheritedIconContentViewModel inherited)
                {
                    if (current == this)
                        return null;

                    if (!visited.Add(current))
                        return null;

                    current = inherited.RealInheritedContent;
                }

                return current;
            }
        }

        /// <summary>
        /// Returns the direct inherited content assigned to this object (not recursively resolved).
        /// </summary>
        public IIconContentViewModel? RealInheritedContent => _inhertiedContent;

        private IDisposable? _parentSubscription;
        private IDisposable? _parentIconSubscription;
        private IDisposable? _inheritedContentChangedSubscription;

        #region Parent setup

        public void SetParent(IObservableFindableIconHolder parent)
        {
            if (parent == BindedParent)
                return;

            DisposeParentSubscriptions();
            BindedParent = parent;
            ParentUid = parent.Uid;
            HookParent(parent);
        }

        public async Task FindAndSetParent(Guid uid, InheritedIcon.InheritedIconParentTypeEnum parentType)
        {
            var icon = await _service.GetIconHolderOrNullAsync(uid, parentType);

            if (icon != null && icon != BindedParent)
                SetParent(icon);
        }

        #endregion

        #region Subscription helpers

        private void HookParent(INotifyPropertyChanged parent)
        {
            // Subscribe to parent property changes (mainly to detect Icon replacement).
            PropertyChangedEventHandler parentHandler = Parent_PropertyChanged;
            parent.PropertyChanged += parentHandler;
            _parentSubscription = new DisposableAction(() => parent.PropertyChanged -= parentHandler);

            HookParentIcon();
        }

        private void HookParentIcon()
        {
            _parentIconSubscription?.Dispose();

            if (BindedParent == null)
            {
                UpdateContentFromParent();
                return;
            }

            var icon = BindedParent.Icon;
            if (icon == null)
            {
                UpdateContentFromParent();
                return;
            }

            // Listen for changes in the parent's Icon (mainly Content changes).
            PropertyChangedEventHandler iconHandler = Icon_PropertyChanged;
            icon.PropertyChanged += iconHandler;
            _parentIconSubscription = new DisposableAction(() => icon.PropertyChanged -= iconHandler);

            UpdateContentFromParent();
        }

        private void Parent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // React to Icon replacement on the parent side.
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IIconHolderViewModel.Icon))
                HookParentIcon();
        }

        private void Icon_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // React to changes of Icon.Content.
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IconViewModel.Content))
                UpdateContentFromParent();
        }

        private void DisposeParentSubscriptions()
        {
            _parentSubscription?.Dispose();
            _parentSubscription = null;

            _parentIconSubscription?.Dispose();
            _parentIconSubscription = null;
        }

        #endregion

        #region Inherited chain logic

        private void UpdateContentFromParent()
        {
            // Get direct content from parent Icon.
            var newReal = BindedParent?.Icon?.Content;

            // If reference differs, rebind subscriptions and update chain state.
            if (!ReferenceEquals(newReal, _inhertiedContent))
            {
                _inheritedContentChangedSubscription?.Dispose();
                _inheritedContentChangedSubscription = null;

                _inhertiedContent = newReal;

                // Subscribe to all nodes of the inherited chain.
                if (_inhertiedContent != null)
                    _inheritedContentChangedSubscription = SubscribeToEntireChain(_inhertiedContent);

                OnPropertyChanged(nameof(RealInheritedContent));
                EvaluateChainChanges();
            }
            else
            {
                // No reference change, but internal fields of the chain nodes may have changed.
                EvaluateChainChanges();
            }
        }

        /// <summary>
        /// Subscribes to PropertyChanged of all nodes in the inheritance chain.
        /// </summary>
        private IDisposable SubscribeToEntireChain(IIconContentViewModel start)
        {
            var disposables = new List<IDisposable>();
            var visited = new HashSet<IIconContentViewModel>();
            IIconContentViewModel? node = start;

            while (node != null)
            {
                if (!visited.Add(node))
                    break;

                if (node is INotifyPropertyChanged inpc)
                {
                    PropertyChangedEventHandler handler = ChainNode_PropertyChanged;
                    inpc.PropertyChanged += handler;
                    disposables.Add(new DisposableAction(() => inpc.PropertyChanged -= handler));
                }

                if (node is InheritedIconContentViewModel inh)
                    node = inh.RealInheritedContent;
                else
                    break;
            }

            return new CompositeDisposable(disposables);
        }

        private void ChainNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Any change inside the chain may affect the bottom content or IsEmpty.
            EvaluateChainChanges();
        }

        private IIconContentViewModel? _cachedInheritedContent;

        /// <summary>
        /// Recomputes the resolved bottom content and IsEmpty.
        /// Notifies UI only when values actually change.
        /// </summary>
        private void EvaluateChainChanges()
        {
            var newBottom = ComputeInheritedContent();
            if (!ReferenceEquals(newBottom, _cachedInheritedContent))
            {
                _cachedInheritedContent = newBottom;
                OnPropertyChanged(nameof(InheritedContent));
            }

            bool newIsEmpty = (newBottom == null) || newBottom.IsEmpty;
            IsEmpty = newIsEmpty;
        }

        /// <summary>
        /// Resolves the deepest actual content (non-inherited) node in the chain.
        /// </summary>
        private IIconContentViewModel? ComputeInheritedContent()
        {
            var current = _inhertiedContent;
            var visited = new HashSet<IIconContentViewModel>();

            while (current is InheritedIconContentViewModel inherited)
            {
                if (current == this)
                    return null;

                if (!visited.Add(current))
                    return null;

                current = inherited.RealInheritedContent;
            }

            return current;
        }

        #endregion

        public void Dispose()
        {
            DisposeParentSubscriptions();
            _inheritedContentChangedSubscription?.Dispose();
            _inheritedContentChangedSubscription = null;
        }

        #region IDisposable helpers

        private class DisposableAction : IDisposable
        {
            private Action? _dispose;
            public DisposableAction(Action dispose) { _dispose = dispose; }
            public void Dispose() { var d = _dispose; _dispose = null; d?.Invoke(); }
        }

        private class CompositeDisposable : IDisposable
        {
            private List<IDisposable>? _disposables;
            public CompositeDisposable(List<IDisposable> disposables) { _disposables = disposables; }
            public void Dispose()
            {
                if (_disposables == null) return;
                foreach (var d in _disposables) d.Dispose();
                _disposables = null;
            }
        }

        #endregion
    }
}