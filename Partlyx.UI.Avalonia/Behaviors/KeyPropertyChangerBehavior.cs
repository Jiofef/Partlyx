using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Globalization;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class KeyPropertyChangerBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<Control?> TargetObjectProperty =
            AvaloniaProperty.Register<KeyPropertyChangerBehavior, Control?>(nameof(TargetObject));

        public Control? TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        public static readonly StyledProperty<Key> KeyProperty =
            AvaloniaProperty.Register<KeyPropertyChangerBehavior, Key>(nameof(Key));

        public Key Key
        {
            get => GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public static readonly StyledProperty<string?> PropertyNameProperty =
            AvaloniaProperty.Register<KeyPropertyChangerBehavior, string?>(nameof(PropertyName));

        public string? PropertyName
        {
            get => GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public static readonly StyledProperty<object?> ValueProperty =
            AvaloniaProperty.Register<KeyPropertyChangerBehavior, object?>(nameof(Value));

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is Control control)
            {
                control.AddHandler(
                    InputElement.KeyDownEvent,
                    OnKeyDown,
                    RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                    true 
                );
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is Control control)
            {
                control.RemoveHandler(
                    InputElement.KeyDownEvent,
                    OnKeyDown
                );
            }

            base.OnDetaching();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key)
            {
                return;
            }

            // Determine the target control: use TargetObject if set, otherwise use AssociatedObject.
            var targetControl = TargetObject ?? AssociatedObject;

            if (targetControl == null || string.IsNullOrWhiteSpace(PropertyName))
            {
                return;
            }

            // Execute the ChangeProperty logic.

            // Find the AvaloniaProperty by name using the correct API.
            AvaloniaProperty? property = AvaloniaPropertyRegistry.Instance.FindRegistered(targetControl.GetType(), PropertyName);

            if (property != null)
            {
                object? convertedValue = Value;

                // Type conversion logic
                if (Value != null && Value.GetType() != property.PropertyType)
                {
                    try
                    {
                        convertedValue = Convert.ChangeType(Value, property.PropertyType, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        // silent
                    }
                }

                // Set the value directly.
                targetControl.SetValue(property, convertedValue);
            }
        }
    }
}