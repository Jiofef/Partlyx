using Microsoft.Extensions.DependencyInjection;
using Partlyx.UI.WPF.DragAndDrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Логика взаимодействия для PartProperties.xaml
    /// </summary>
    public partial class PartProperties : UserControl
    {
        public PartProperties()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object s, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            var handler = App.Services.GetRequiredService<PartsTreeDropHandler>();
            GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(PropertiesListView, handler);
        }
    }
}
