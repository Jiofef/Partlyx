using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class DoubleTappedBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand?> CommandProperty =
            AvaloniaProperty.Register<DoubleTappedBehavior, ICommand?>(nameof(Command));

        public ICommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<object?> CommandParameterProperty =
            AvaloniaProperty.Register<DoubleTappedBehavior, object?>(nameof(CommandParameter));

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly StyledProperty<bool> MarkAsHandledProperty =
            AvaloniaProperty.Register<DoubleTappedBehavior, bool>(nameof(MarkAsHandled), defaultValue: true);

        public bool MarkAsHandled
        {
            get => GetValue(MarkAsHandledProperty);
            set => SetValue(MarkAsHandledProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.DoubleTapped += OnDoubleTapped;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.DoubleTapped -= OnDoubleTapped;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);

                if (MarkAsHandled)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
