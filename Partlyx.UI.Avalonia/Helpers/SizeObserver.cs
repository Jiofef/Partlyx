using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Partlyx.UI.Avalonia.Helpers
{
    public class SizeObserver
    {
        // BindableWidth
        public static readonly AttachedProperty<double> BindableWidthProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, double>(
                "BindableWidth", default(double), defaultBindingMode: BindingMode.TwoWay);
        public static double GetBindableWidth(AvaloniaObject obj) => obj.GetValue(BindableWidthProperty);
        public static void SetBindableWidth(AvaloniaObject obj, double value) => obj.SetValue(BindableWidthProperty, value);

        // BindableHeight
        public static readonly AttachedProperty<double> BindableHeightProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, double>(
                "BindableHeight", default(double), defaultBindingMode: BindingMode.TwoWay);
        public static double GetBindableHeight(AvaloniaObject obj) => obj.GetValue(BindableHeightProperty);
        public static void SetBindableHeight(AvaloniaObject obj, double value) => obj.SetValue(BindableHeightProperty, value);

        // Observe
        public static readonly AttachedProperty<bool> ObserveProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, Control, bool>(
                "Observe", false);
        public static bool GetObserve(Control obj) => obj.GetValue(ObserveProperty);
        public static void SetObserve(Control obj, bool value) => obj.SetValue(ObserveProperty, value);

        // SizeChangedCommand
        public static readonly AttachedProperty<ICommand?> SizeChangedCommandProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, ICommand?>(
                "SizeChangedCommand", null);
        public static ICommand? GetSizeChangedCommand(AvaloniaObject obj) => obj.GetValue(SizeChangedCommandProperty);
        public static void SetSizeChangedCommand(AvaloniaObject obj, ICommand? value) => obj.SetValue(SizeChangedCommandProperty, value);

        // SizeChangedCommandParameter
        public static readonly AttachedProperty<object?> SizeChangedCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, object?>(
                "SizeChangedCommandParameter", null);
        public static object? GetSizeChangedCommandParameter(AvaloniaObject obj) => obj.GetValue(SizeChangedCommandParameterProperty);
        public static void SetSizeChangedCommandParameter(AvaloniaObject obj, object? value) => obj.SetValue(SizeChangedCommandParameterProperty, value);

        // OneShotCommand (default true to match original)
        public static readonly AttachedProperty<bool> OneShotCommandProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, bool>(
                "OneShotCommand", true);
        public static bool GetOneShotCommand(AvaloniaObject obj) => obj.GetValue(OneShotCommandProperty);
        public static void SetOneShotCommand(AvaloniaObject obj, bool value) => obj.SetValue(OneShotCommandProperty, value);

        // HasShoted (private marker)
        private static readonly AttachedProperty<bool> HasShotedProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, AvaloniaObject, bool>(
                "HasShoted", false);
        private static bool GetHasShoted(AvaloniaObject obj) => obj.GetValue(HasShotedProperty);
        private static void SetHasShoted(AvaloniaObject obj, bool value) => obj.SetValue(HasShotedProperty, value);

        // subscription holder per control
        private static readonly AttachedProperty<IDisposable?> SubscriptionProperty =
            AvaloniaProperty.RegisterAttached<SizeObserver, Control, IDisposable?>(
                "Subscription", null);
        private static IDisposable? GetSubscription(Control c) => c.GetValue(SubscriptionProperty);
        private static void SetSubscription(Control c, IDisposable? d) => c.SetValue(SubscriptionProperty, d);

        static SizeObserver()
        {
            // react to Observe property changes
            ObserveProperty.Changed.Subscribe(args =>
            {
                if (args.Sender is Control ctrl)
                {
                    var observe = args.NewValue.Value;
                    OnObserveChanged(ctrl, observe);
                }
            });
        }

        private static void OnObserveChanged(Control ctrl, bool observe)
        {
            // dispose previous subscription if any
            GetSubscription(ctrl)?.Dispose();
            SetSubscription(ctrl, null);

            if (!observe)
                return;

            var disp = new CompositeDisposable();

            // Observable for bounds changes
            var boundsObs = ctrl.GetObservable(Control.BoundsProperty)
                                .Select(r => (width: r.Width, height: r.Height))
                                .DistinctUntilChanged();

            // Subscribe bounds changes
            var sub = boundsObs.Subscribe(t =>
            {
                UpdateAndExecuteCommandIfNeeded(ctrl, t.width, t.height);
            });
            disp.Add(sub);

            // Try immediate update: if control already has non-zero size - update now.
            if (ctrl.Bounds.Width > 0 || ctrl.Bounds.Height > 0)
            {
                UpdateAndExecuteCommandIfNeeded(ctrl, ctrl.Bounds.Width, ctrl.Bounds.Height);
            }
            else
            {
                // If size is zero, wait until control is attached to logical tree, then post an update on UI thread.
                void OnAttached(object? s, LogicalTreeAttachmentEventArgs e)
                {
                    ctrl.AttachedToLogicalTree -= OnAttached;
                    // Post to dispatcher to let layout run
                    Dispatcher.UIThread.Post(() =>
                    {
                        UpdateAndExecuteCommandIfNeeded(ctrl, ctrl.Bounds.Width, ctrl.Bounds.Height);
                    }, DispatcherPriority.Background);
                }

                ctrl.AttachedToLogicalTree += OnAttached;
                // ensure we remove handler when disposing
                disp.Add(Disposable.Create(() => ctrl.AttachedToLogicalTree -= OnAttached));
            }

            SetSubscription(ctrl, disp);
        }

        private static void UpdateAndExecuteCommandIfNeeded(AvaloniaObject obj, double width, double height)
        {
            if (double.IsNaN(width)) width = 0;
            if (double.IsNaN(height)) height = 0;

            obj.SetValue(BindableWidthProperty, width);
            obj.SetValue(BindableHeightProperty, height);

            var command = GetSizeChangedCommand(obj);
            if (command == null) return;

            bool fireOnce = GetOneShotCommand(obj);
            if (fireOnce && GetHasShoted(obj)) return;

            var param = GetSizeChangedCommandParameter(obj);

            // Ensure invocation on UI thread (Avalonia dispatcher)
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() =>
                {
                    TryExecuteCommand(command, param);
                    SetHasShoted(obj, true);
                }, DispatcherPriority.Background);
            }
            else
            {
                TryExecuteCommand(command, param);
                SetHasShoted(obj, true);
            }
        }

        private static void TryExecuteCommand(ICommand command, object? parameter)
        {
            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}
