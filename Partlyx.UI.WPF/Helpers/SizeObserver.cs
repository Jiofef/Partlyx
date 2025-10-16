using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Partlyx.UI.WPF.Helpers
{
    public static class SizeObserver
    {
        public static readonly DependencyProperty BindableWidthProperty =
            DependencyProperty.RegisterAttached("BindableWidth", typeof(double), typeof(SizeObserver),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static double GetBindableWidth(DependencyObject obj) => (double)obj.GetValue(BindableWidthProperty);
        public static void SetBindableWidth(DependencyObject obj, double value) => obj.SetValue(BindableWidthProperty, value);

        public static readonly DependencyProperty BindableHeightProperty =
            DependencyProperty.RegisterAttached("BindableHeight", typeof(double), typeof(SizeObserver),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static double GetBindableHeight(DependencyObject obj) => (double)obj.GetValue(BindableHeightProperty);
        public static void SetBindableHeight(DependencyObject obj, double value) => obj.SetValue(BindableHeightProperty, value);

        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver),
                new PropertyMetadata(false, OnObserveChanged));
        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);
        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);

        public static readonly DependencyProperty SizeChangedCommandProperty =
            DependencyProperty.RegisterAttached("SizeChangedCommand", typeof(ICommand), typeof(SizeObserver),
                new PropertyMetadata(null));
        public static ICommand GetSizeChangedCommand(DependencyObject obj) => (ICommand)obj.GetValue(SizeChangedCommandProperty);
        public static void SetSizeChangedCommand(DependencyObject obj, ICommand value) => obj.SetValue(SizeChangedCommandProperty, value);

        public static readonly DependencyProperty SizeChangedCommandParameterProperty =
            DependencyProperty.RegisterAttached("SizeChangedCommandParameter", typeof(object), typeof(SizeObserver),
                new PropertyMetadata(null));
        public static object GetSizeChangedCommandParameter(DependencyObject obj) => obj.GetValue(SizeChangedCommandParameterProperty);
        public static void SetSizeChangedCommandParameter(DependencyObject obj, object value) => obj.SetValue(SizeChangedCommandParameterProperty, value);

        public static readonly DependencyProperty OneShotCommandProperty =
            DependencyProperty.RegisterAttached("OneShotCommand", typeof(bool), typeof(SizeObserver), new PropertyMetadata(true));
        public static bool GetOneShotCommand(DependencyObject obj) => (bool)obj.GetValue(OneShotCommandProperty);
        public static void SetOneShotCommand(DependencyObject obj, bool value) => obj.SetValue(OneShotCommandProperty, value);

        private static readonly DependencyProperty HasShotedProperty =
            DependencyProperty.RegisterAttached("HasShoted", typeof(bool), typeof(SizeObserver), new PropertyMetadata(false));
        private static bool GetHasShoted(DependencyObject obj) => (bool)obj.GetValue(HasShotedProperty);
        private static void SetHasShoted(DependencyObject obj, bool value) => obj.SetValue(HasShotedProperty, value);

        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement fe)) return;

            if ((bool)e.NewValue)
            {
                fe.SizeChanged += Fe_SizeChanged;
                TrySetCurrentSize(fe);
                if (fe.ActualWidth == 0 && fe.ActualHeight == 0)
                {
                    RoutedEventHandler loaded = null;
                    loaded = (s, args) =>
                    {
                        fe.Loaded -= loaded;
                        fe.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UpdateAndExecuteCommandIfNeeded(fe, fe.ActualWidth, fe.ActualHeight);
                        }), DispatcherPriority.Loaded);
                    };
                    fe.Loaded += loaded;
                }
            }
            else
            {
                fe.SizeChanged -= Fe_SizeChanged;
            }
        }

        private static void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement fe)
                UpdateAndExecuteCommandIfNeeded(fe, e.NewSize.Width, e.NewSize.Height);
        }

        private static void TrySetCurrentSize(FrameworkElement fe)
        {
            if (fe.ActualWidth > 0 || fe.ActualHeight > 0)
            {
                UpdateAndExecuteCommandIfNeeded(fe, fe.ActualWidth, fe.ActualHeight);
            }
            else
            {
                fe.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateAndExecuteCommandIfNeeded(fe, fe.ActualWidth, fe.ActualHeight);
                }), DispatcherPriority.Loaded);
            }
        }

        private static void UpdateAndExecuteCommandIfNeeded(DependencyObject obj, double width, double height)
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
            var dispatcherObj = obj as DispatcherObject;
            if (dispatcherObj == null)
            {
                TryExecuteCommand(command, param);
                SetHasShoted(obj, true);
                return;
            }

            dispatcherObj.Dispatcher.BeginInvoke(new Action(() =>
            {
                TryExecuteCommand(command, param);
                SetHasShoted(obj, true);
            }), DispatcherPriority.DataBind);
        }

        private static void TryExecuteCommand(ICommand command, object parameter)
        {
            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}