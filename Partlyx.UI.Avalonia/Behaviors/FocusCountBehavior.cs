using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class FocusCountBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<int> CountProperty =
            AvaloniaProperty.Register<FocusCountBehavior, int>(nameof(Count));

        public static readonly StyledProperty<int> WeightProperty =
            AvaloniaProperty.Register<FocusCountBehavior, int>(nameof(Weight), 1);

        bool _isFocused;

        public int Count
        {
            get => GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public int Weight
        {
            get => GetValue(WeightProperty);
            set => SetValue(WeightProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.GotFocus += AssociatedObject_GotFocus;
                AssociatedObject.LostFocus += AssociatedObject_LostFocus;
                if (AssociatedObject.IsFocused)
                {
                    _isFocused = true;
                    Count += Weight;
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
                AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
                if (_isFocused)
                {
                    Count -= Weight;
                    _isFocused = false;
                }
            }
        }

        private void AssociatedObject_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (_isFocused) return;
            Count += Weight;
            _isFocused = true;
        }

        private void AssociatedObject_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!_isFocused) return;
            Count -= Weight;
            _isFocused = false;
        }
    }
}
