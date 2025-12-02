using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TreeViewItemContainerSelectionSyncBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<string?> TargetPropertyNameProperty =
            AvaloniaProperty.Register<TreeViewItemContainerSelectionSyncBehavior, string?>(
                nameof(TargetPropertyName), defaultValue: "IsSelected");

        public string? TargetPropertyName
        {
            get => GetValue(TargetPropertyNameProperty);
            set => SetValue(TargetPropertyNameProperty, value);
        }
        // ...

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            }
            base.OnDetaching();
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            TreeViewItem? container = AssociatedObject?.FindAncestorOfType<TreeViewItem>();
            string? targetPropName = TargetPropertyName;

            if (container != null && AssociatedObject!.DataContext != null && !string.IsNullOrEmpty(targetPropName))
            {
                var binding = new Binding(targetPropName)
                {
                    Mode = BindingMode.TwoWay,
                    Source = AssociatedObject.DataContext
                };

                container.Bind(TreeViewItem.IsSelectedProperty, binding);
            }
        }
    }
}
