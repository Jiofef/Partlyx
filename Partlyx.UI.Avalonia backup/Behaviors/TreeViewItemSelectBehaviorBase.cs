using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public abstract class TreeViewItemSelectBehaviorBase : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectedItemChanged -= OnSelectedItemChanged;
        }

        protected virtual void OnSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e) { }
    }
}
