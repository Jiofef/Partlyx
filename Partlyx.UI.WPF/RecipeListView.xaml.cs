using Microsoft.Extensions.DependencyInjection;
using Partlyx.Infrastructure.Data.Interfaces;
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

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Логика взаимодействия для ResourceListView.xaml
    /// </summary>
    public partial class RecipeListView : UserControl
    {
        public RecipeListView()
        {
            InitializeComponent();
        }

        private void NameBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (sender is TextBlock tb && tb.DataContext is RecipeItemViewModel vm)
                    vm.Ui.IsRenaming = true;
            }
        }
    }
}
