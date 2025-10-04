using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Partlyx.UI.WPF.Behaviors
{
    public static class ItemDoubleClickBehavior
    {
        
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommand", typeof(ICommand), typeof(ItemDoubleClickBehavior));

        public static ICommand GetDoubleClickCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(DoubleClickCommandProperty);
        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(DoubleClickCommandProperty, value);


        public static readonly DependencyProperty DoubleClickCommandParameterProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommandParameter", typeof(object), typeof(ItemDoubleClickBehavior));

        public static object GetDoubleClickCommandParameter(DependencyObject obj) =>
            obj.GetValue(DoubleClickCommandParameterProperty);
        public static void SetDoubleClickCommandParameter(DependencyObject obj, object value) =>
            obj.SetValue(DoubleClickCommandParameterProperty, value);

        static ItemDoubleClickBehavior()
        {
            EventManager.RegisterClassHandler(typeof(ListViewItem), Control.MouseDoubleClickEvent,
                    new RoutedEventHandler(OnDoubleClick));
        }

        private static void OnDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem item && GetDoubleClickCommand(item) is ICommand cmd)
            {
                var param = GetDoubleClickCommandParameter(item) ?? item.DataContext;
                if (cmd.CanExecute(param)) cmd.Execute(param);
            }
            e.Handled = true;
        }
    }
}
