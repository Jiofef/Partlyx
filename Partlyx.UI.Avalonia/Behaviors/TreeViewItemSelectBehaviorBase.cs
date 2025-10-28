using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public abstract class TreeViewItemSelectBehaviorBase : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
                AssociatedObject.SelectionChanged += OnSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
                AssociatedObject.SelectionChanged -= OnSelectedItemChanged;
        }

        protected virtual void OnSelectedItemChanged(object? sender, SelectionChangedEventArgs e) { }
    }
}
