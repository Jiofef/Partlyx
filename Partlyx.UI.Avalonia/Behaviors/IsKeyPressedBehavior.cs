using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class IsKeyPressedBehavior : Behavior<InputElement>
    {
        public static readonly StyledProperty<Key> KeyProperty =
            AvaloniaProperty.Register<IsKeyPressedBehavior, Key>(nameof(Key));

        public Key Key
        {
            get => GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public static readonly StyledProperty<bool> IsPressedProperty =
            AvaloniaProperty.Register<IsKeyPressedBehavior, bool>(nameof(IsPressed), defaultBindingMode: BindingMode.TwoWay);

        public bool IsPressed
        {
            get => GetValue(IsPressedProperty);
            set => SetValue(IsPressedProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.KeyDown += OnKeyDown;
                AssociatedObject.KeyUp += OnKeyUp;
                AssociatedObject.LostFocus += OnLostFocus;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.KeyDown -= OnKeyDown;
                AssociatedObject.KeyUp -= OnKeyUp;
                AssociatedObject.LostFocus -= OnLostFocus;
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key)
            {
                IsPressed = true;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key)
            {
                IsPressed = false;
            }
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            IsPressed = false;
        }
    }
}
