using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System;
using System.Windows.Input;

namespace Partlyx.UI.Avalonia.Behaviors
{
    public class DragHoverCommandBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<int> DelayMsProperty =
            AvaloniaProperty.Register<DragHoverCommandBehavior, int>(nameof(DelayMs), defaultValue:500);
        private DispatcherTimer? _timer;
        public int DelayMs
        {
            get => GetValue(DelayMsProperty);
            set
            {
                SetValue(DelayMsProperty, value);
                Delay = TimeSpan.FromMilliseconds(value);
            }
        }

        public static readonly StyledProperty<bool> EnabledProperty =
            AvaloniaProperty.Register<DragHoverCommandBehavior, bool>(nameof(Enabled), defaultValue: true);

        public bool Enabled
        {
            get => GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }

        public static readonly StyledProperty<ICommand?> CommandProperty =
            AvaloniaProperty.Register<DragHoverCommandBehavior, ICommand?>(nameof(Command));

        public ICommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<object?> CommandParameterProperty =
            AvaloniaProperty.Register<DragHoverCommandBehavior, object?>(nameof(CommandParameter));

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly StyledProperty<TimeSpan> DelayProperty =
            AvaloniaProperty.Register<DragHoverCommandBehavior, TimeSpan>(nameof(Delay), defaultValue: TimeSpan.FromMilliseconds(500));

        public TimeSpan Delay
        {
            get => GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble);
                AssociatedObject.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble);
                AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AssociatedObject.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
                AssociatedObject.RemoveHandler(DragDrop.DropEvent, OnDrop);
            }
            StopTimer();
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (Enabled)
                StartTimer();
        }

        private void OnDragLeave(object? sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            StopTimer();
        }

        private void StartTimer()
        {
            StopTimer();

            if (DelayMs <= 0)
            {
                TryExcecuteCommand();
                return;
            }

            _timer = new DispatcherTimer
            {
                Interval = Delay
            };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= TimerOnTick;
                _timer = null;
            }
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            StopTimer();
            TryExcecuteCommand();
        }

        private void TryExcecuteCommand()
        {
            if (Enabled)
            {
                object? parameter = CommandParameter;

                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }
    }
}
