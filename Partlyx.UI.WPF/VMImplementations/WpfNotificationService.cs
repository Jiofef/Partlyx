using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Partlyx.UI.WPF.Helpers.UIThreadHelper;

namespace Partlyx.UI.WPF.VMImplementations
{
    public class WpfNotificationService : INotificationService
    {
        public Task<bool> ShowConfirmAsync(string title, string message, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var owner = GetActiveWindow();
                var result = MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }

        public Task ShowErrorAsync(string title, string message, string? details = null, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var text = message;
                if (!string.IsNullOrEmpty(details))
                    text += Environment.NewLine + Environment.NewLine + "Details:" + details;

                var owner = GetActiveWindow();
                MessageBox.Show(owner, text, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public Task ShowInfoAsync(string title, string message, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var owner = GetActiveWindow();
                MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private Window? GetActiveWindow() => Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }
}
