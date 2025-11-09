using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class TextBoxFocusAndSelectBehavior : Behavior<TextBox>
    {
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<TextBoxFocusAndSelectBehavior, bool>(nameof(IsActive));

        /// <summary>
        /// If true, Focus() + selectAll() will be called when switching to true.
        /// </summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        // You can set the priority of a delayed call (default is Render).
        public static readonly StyledProperty<DispatcherPriority> PriorityProperty =
            AvaloniaProperty.Register<TextBoxFocusAndSelectBehavior, DispatcherPriority>(nameof(Priority), DispatcherPriority.Render);

        public DispatcherPriority Priority
        {
            get => GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            // we monitor the change of the IsActive property inside the behavior
            this.GetObservable(IsActiveProperty).Subscribe(OnIsActiveChanged);

            // Если при прикреплении уже true — активируем
            if (IsActive)
                PostFocusSelect();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            // subscriptions via GetObservable do not require manual unsubscribing in a simple case,
            // but if you want, you can save the IDisposable and Dispose() is here.
        }

        private void OnIsActiveChanged(bool value)
        {
            if (value)
                PostFocusSelect();
        }

        private void PostFocusSelect()
        {
            var tb = AssociatedObject;
            if (tb == null) return;

            // Delaying so that the TextBox is already visible in the tree
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (!tb.IsVisible) return;
                    tb.Focus();
                    tb.SelectAll();
                }
                catch
                {
                    // silent
                }
            }, Priority);
        }
    }
}
