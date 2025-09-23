using Partlyx.ViewModels.UIObjectViewModels;
using System.ComponentModel;
using System.Windows;

namespace Partlyx.UI.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool _confirmedClose;
        protected override async void OnClosing(CancelEventArgs e)
        {
            if (_confirmedClose)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;

            try
            {
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    _confirmedClose = await vm.ConfirmClosingAsync();
                    if (!_confirmedClose) return;
                }

                // If we just call Close(), nothing will happen because of previously used await and threads, so we use Dispatcher
                _ = Dispatcher.BeginInvoke((Action)(() => Close()),
                       System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch { }
        }
    }
}