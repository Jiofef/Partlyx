using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class SelectionListSynchronizationBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<SelectionListSynchronizationBehavior, bool>(nameof(IsActive));

        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly StyledProperty<IList?> SelectedItemsListProperty =
            AvaloniaProperty.Register<SelectionListSynchronizationBehavior, IList?>(nameof(SelectedItemsList));

        public IList? SelectedItemsList
        {
            get => GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly StyledProperty<bool> ShouldClearOthersProperty =
            AvaloniaProperty.Register<SelectionListSynchronizationBehavior, bool>(nameof(ShouldClearOthers), defaultValue: false);

        public bool ShouldClearOthers
        {
            get => GetValue(ShouldClearOthersProperty);
            set => SetValue(ShouldClearOthersProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.GetObservable(IsActiveProperty).Subscribe(IsActiveChanged);
        }


        private void IsActiveChanged(bool isActive)
        {
            if (AssociatedObject == null || SelectedItemsList == null)
                return;

            object? item = AssociatedObject.DataContext;

            if (item == null)
                return;

            if (isActive)
            {
                if (!SelectedItemsList.Contains(item))
                {
                    if (ShouldClearOthers)
                    {
                        SelectedItemsList.Clear();
                    }

                    SelectedItemsList.Add(item);
                }
            }
            else
            {
                if (SelectedItemsList.Contains(item))
                {
                    SelectedItemsList.Remove(item);
                }
            }
        }
    }
}
