using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Material.Styles.Themes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Partlyx.UI.Avalonia.OtherControls
{
    public class VirtualizingWrapPanel : VirtualizingPanel
    {
        public static readonly StyledProperty<double> ItemHeightProperty =
            AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemHeight), 40.0);

        public double ItemHeight
        {
            get => GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly StyledProperty<double> ItemWidthProperty =
            AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemWidth), 40.0);

        public double ItemWidth
        {
            get => GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly StyledProperty<bool> DisableContainerReuseProperty =
            AvaloniaProperty.Register<VirtualizingWrapPanel, bool>(nameof(DisableContainerReuse), false);

        public bool DisableContainerReuse
        {
            get => GetValue(DisableContainerReuseProperty);
            set => SetValue(DisableContainerReuseProperty, value);
        }

        private int _firstRealizedIndex = 0;
        private double _measuredItemHeight = 0.0;
        private double _measuredItemWidth = 0.0;
        private bool _measuredSizeJustSet = false;

        // recycleKey -> stack of containers
        private readonly Dictionary<object, Stack<Control>> _recyclePools = new();
        // container -> recycleKey
        private readonly Dictionary<Control, object?> _containerRecycleKey = new();

        public VirtualizingWrapPanel()
        {
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }


        protected override Size MeasureOverride(Size availableSize)
        {
            var itemsControl = ItemsControl ?? throw new InvalidOperationException("Panel is not hosted in an ItemsControl");
            var generator = ItemContainerGenerator ?? throw new InvalidOperationException("ItemContainerGenerator is null");

            int itemsCount = itemsControl.ItemCount;
            if (itemsCount == 0)
            {
                RemoveInternalChildRange(0, Children.Count);
                _firstRealizedIndex = 0;
                return new Size(0, 0);
            }

            double itemH = _measuredItemHeight > 0 ? _measuredItemHeight : ItemHeight;
            if (double.IsNaN(itemH) || itemH <= 0)
                itemH = ItemHeight;

            double itemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;
            if (double.IsNaN(itemW) || itemW <= 0)
                itemW = ItemWidth;

            int itemsPerRow = 1;
            if (!double.IsInfinity(availableSize.Width) && itemW > 0)
            {
                itemsPerRow = Math.Max(1, (int)(availableSize.Width / itemW));
            }

            double viewportHeight = availableSize.Height;
            double viewportOffsetY = 0;
            var sv = this.FindAncestorOfType<ScrollViewer>();
            if (sv != null)
            {
                viewportOffsetY = sv.Offset.Y;
                if (double.IsInfinity(viewportHeight) || viewportHeight <= 0)
                {
                    if (!double.IsInfinity(sv.Viewport.Height) && sv.Viewport.Height > 0)
                        viewportHeight = sv.Viewport.Height;
                    else if (sv.Bounds.Height > 0)
                        viewportHeight = sv.Bounds.Height;
                }
            }
            if (double.IsInfinity(viewportHeight) || viewportHeight <= 0)
                viewportHeight = itemH * 3;

            int firstVisibleRow = (int)(viewportOffsetY / itemH);
            int visibleRowsCount = (int)Math.Ceiling(viewportHeight / itemH) + 2;
            int targetIndex = firstVisibleRow * itemsPerRow;
            int desiredFirstIndex = Math.Max(0, Math.Min(targetIndex, Math.Max(0, itemsCount - 1)));

            if (Children.Count == 0)
            {
                _firstRealizedIndex = desiredFirstIndex;
            }

            int desiredLastIndex = Math.Min(itemsCount - 1, desiredFirstIndex + (visibleRowsCount * itemsPerRow) - 1);

            while (Children.Count > 0 && IndexFromContainer(Children[0] as Control) < desiredFirstIndex)
            {
                var child = Children[0] as Control;
                if (child != null && ItemContainerGenerator != null)
                {
                    RecycleContainer(child, ItemContainerGenerator);
                    RemoveInternalChild(child);
                }
                else
                {
                    RemoveInternalChildRange(0, 1);
                }
                _firstRealizedIndex++;
            }

            // Удаляем с конца лишние
            int currentLastIndex = Children.Count > 0 ? IndexFromContainer(Children[Children.Count - 1] as Control) : -1;
            while (currentLastIndex > desiredLastIndex && Children.Count > 0)
            {
                var child = Children[Children.Count - 1] as Control;
                if (child != null && ItemContainerGenerator != null)
                {
                    RecycleContainer(child, ItemContainerGenerator);
                    RemoveInternalChild(child);
                }
                else
                {
                    RemoveInternalChildRange(Children.Count - 1, 1);
                }

                currentLastIndex = Children.Count > 0 ? IndexFromContainer(Children[Children.Count - 1] as Control) : -1;
            }

            int currentFirstIndex = Children.Count > 0 ? IndexFromContainer(Children[0] as Control) : desiredFirstIndex;
            if (Children.Count == 0)
            {
                _firstRealizedIndex = desiredFirstIndex;
                currentFirstIndex = desiredFirstIndex;
            }

            while (currentFirstIndex > desiredFirstIndex)
            {
                currentFirstIndex--;
                int itemIndex = currentFirstIndex;
                Control container = CreateOrRecycleContainer(itemsControl, generator, itemIndex);
                InsertInternalChild(0, container);
                _firstRealizedIndex--;
            }

            currentLastIndex = Children.Count > 0 ? IndexFromContainer(Children[Children.Count - 1] as Control) : desiredFirstIndex - 1;
            for (int itemIndex = currentLastIndex + 1; itemIndex <= desiredLastIndex; itemIndex++)
            {
                Control container = CreateOrRecycleContainer(itemsControl, generator, itemIndex);
                AddInternalChild(container);
            }

            if ((_measuredItemHeight <= 0 || _measuredItemWidth <= 0) && Children.Count > 0)
            {
                var container = Children[0];
                container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var h = container.DesiredSize.Height;
                var w = container.DesiredSize.Width;
                if (h > 0 && !double.IsNaN(h)) _measuredItemHeight = h;
                if (w > 0 && !double.IsNaN(w)) _measuredItemWidth = w;
                _measuredSizeJustSet = true;
            }

            if (_measuredSizeJustSet)
            {
                _measuredSizeJustSet = false;
                InvalidateMeasure();
            }

            double finalItemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;
            double finalItemH = _measuredItemHeight > 0 ? _measuredItemHeight : ItemHeight;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                int itemIndex = _firstRealizedIndex + i;
                object? item = GetItemAt(itemsControl, itemIndex);
                generator.PrepareItemContainer(child, item, itemIndex);
                child.Measure(new Size(finalItemW, finalItemH));
            }

            int totalRowsCount = (int)Math.Ceiling((double)itemsCount / itemsPerRow);
            double totalHeight = totalRowsCount * finalItemH;
            return new Size(availableSize.Width, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double itemH = _measuredItemHeight > 0 ? _measuredItemHeight : ItemHeight;
            double itemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;

            int itemsPerRow = 1;
            if (finalSize.Width > 0 && itemW > 0)
                itemsPerRow = Math.Max(1, (int)(finalSize.Width / itemW));

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                int globalIndex = _firstRealizedIndex + i;
                int row = globalIndex / itemsPerRow;
                int col = globalIndex % itemsPerRow;
                double x = col * itemW;
                double y = row * itemH;
                child.Arrange(new Rect(x, y, itemW, itemH));
            }

            var itemsCtrl = ItemsControl;
            int count = itemsCtrl?.ItemCount ?? 0;
            int totalRows = (int)Math.Ceiling((double)count / itemsPerRow);
            return new Size(finalSize.Width, totalRows * itemH);
        }

        private Control CreateOrRecycleContainer(ItemsControl itemsControl, ItemContainerGenerator generator, int itemIndex)
        {
            object? item = GetItemAt(itemsControl, itemIndex);
            object? recycleKey = null;
            bool needsContainer = generator.NeedsContainer(item, itemIndex, out recycleKey);

            Control container;

            if (needsContainer)
            {
                if (!DisableContainerReuse &&
                    recycleKey != null &&
                    _recyclePools.TryGetValue(recycleKey, out var pool) &&
                    pool.Count > 0)
                {
                    container = pool.Pop();
                }
                else
                {
                    container = generator.CreateContainer(item, itemIndex, recycleKey)
                        ?? throw new InvalidOperationException("CreateContainer returned null");
                }

                _containerRecycleKey[container] = recycleKey;
            }
            else
            {
                if (item is Control c)
                {
                    container = c;
                }
                else
                {
                    container = generator.CreateContainer(item, itemIndex, null)
                        ?? throw new InvalidOperationException("CreateContainer returned null");
                }

                _containerRecycleKey[container] = null;
            }

            generator.PrepareItemContainer(container, item, itemIndex);
            generator.ItemContainerPrepared(container, item, itemIndex);

            return container;
        }

        private void RecycleContainer(Control child, ItemContainerGenerator generator)
        {
            if (DisableContainerReuse)
            {
                generator.ClearItemContainer(child);
                _containerRecycleKey.Remove(child);
                return;
            }

            generator.ClearItemContainer(child);

            if (_containerRecycleKey.TryGetValue(child, out var key) && key != null)
            {
                if (!_recyclePools.TryGetValue(key, out var stack))
                {
                    stack = new Stack<Control>();
                    _recyclePools[key] = stack;
                }

                stack.Push(child);
            }

            _containerRecycleKey.Remove(child);
        }

        private object? GetItemAt(ItemsControl itemsControl, int index)
        {
            if (itemsControl.ItemsView is IReadOnlyList<object?> list)
            {
                if (index >= 0 && index < list.Count)
                    return list[index];
            }
            else if (itemsControl.Items is System.Collections.IList rawList)
            {
                if (index >= 0 && index < rawList.Count)
                    return rawList[index];
            }

            return null;
        }

        protected override Control? ContainerFromIndex(int index)
        {
            int local = index - _firstRealizedIndex;
            if (local >= 0 && local < Children.Count)
                return Children[local] as Control;
            return null;
        }

        protected override int IndexFromContainer(Control container)
        {
            int local = Children.IndexOf(container);
            if (local >= 0)
                return _firstRealizedIndex + local;
            return -1;
        }

        protected override IEnumerable<Control>? GetRealizedContainers()
        {
            return Children.Cast<Control>().ToList();
        }

        protected override Control? ScrollIntoView(int index)
        {
            var itemsCtrl = ItemsControl ?? throw new InvalidOperationException("Panel not hosted in an ItemsControl");
            if (index < 0 || index >= itemsCtrl.ItemCount)
                return null;

            var existing = ContainerFromIndex(index);
            if (existing != null)
                return existing;

            var sv = this.FindAncestorOfType<ScrollViewer>();
            if (sv != null)
            {
                double itemH = _measuredItemHeight > 0 ? _measuredItemHeight : ItemHeight;
                double itemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;
                int itemsPerRow = Math.Max(1, (int)(Bounds.Width / itemW));
                int row = index / itemsPerRow;
                sv.Offset = new Vector(sv.Offset.X, row * itemH);
            }

            return null;
        }

        protected override Control? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            var itemsCtrl = ItemsControl;
            if (itemsCtrl == null || itemsCtrl.ItemCount == 0) return null;

            Control? fromControl = from as Control;
            int startIndex = fromControl != null ? IndexFromContainer(fromControl) : -1;
            int nextIndex = startIndex;

            if (direction == NavigationDirection.Next)
                nextIndex = (startIndex >= 0) ? startIndex + 1 : _firstRealizedIndex;
            else if (direction == NavigationDirection.Previous)
                nextIndex = (startIndex >= 0) ? startIndex - 1 : _firstRealizedIndex - 1;
            else if (direction == NavigationDirection.Down)
            {
                double itemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;
                int itemsPerRow = Math.Max(1, (int)(Bounds.Width / itemW));
                nextIndex = startIndex + itemsPerRow;
            }
            else if (direction == NavigationDirection.Up)
            {
                double itemW = _measuredItemWidth > 0 ? _measuredItemWidth : ItemWidth;
                int itemsPerRow = Math.Max(1, (int)(Bounds.Width / itemW));
                nextIndex = startIndex - itemsPerRow;
            }

            if (wrap)
            {
                if (nextIndex < 0) nextIndex = itemsCtrl.ItemCount - 1;
                if (nextIndex >= itemsCtrl.ItemCount) nextIndex = 0;
            }

            if (nextIndex < 0 || nextIndex >= itemsCtrl.ItemCount)
                return null;

            return ScrollIntoView(nextIndex);
        }
        protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(items, e);

            _firstRealizedIndex = 0;
            _measuredItemHeight = 0;
            _measuredItemWidth = 0;
            _measuredSizeJustSet = false;

            if (Children.Count > 0)
            {
                var generator = ItemContainerGenerator;
                if (generator != null)
                {
                    foreach (var child in Children.OfType<Control>())
                        RecycleContainer(child, generator);
                }

                RemoveInternalChildRange(0, Children.Count);
            }

            _recyclePools.Clear();
            _containerRecycleKey.Clear();

            InvalidateMeasure();
        }

        private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
