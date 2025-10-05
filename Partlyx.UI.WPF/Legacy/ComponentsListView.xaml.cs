using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.UI.WPF.DragAndDrop;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System;
using System.Collections.Generic;
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
using static MaterialDesignThemes.Wpf.Theme;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Логика взаимодействия для ComponentListView.xaml
    /// </summary>
    public partial class ComponentListView : UserControl
    {
        public ComponentListView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object s, RoutedEventArgs e)
        {
            var handler = App.Services.GetRequiredService<RecipeComponentsListDropHandler>();
            GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(ListViewControl, handler);
        }
    }
}
