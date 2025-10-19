using Partlyx.Core.Contracts;
using Partlyx.ViewModels.UIServices;
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
        private readonly ILocalizationService _loc;
        public WpfNotificationService(ILocalizationService loc) 
        {
            _loc = loc;
        }

        public Task<bool> ShowYesNoConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var owner = GetActiveWindow();
                var result = MessageBox.Show(owner, _loc[options.message], _loc[options.title], MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }

        public Task<bool?> ShowYesNoCancelConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var owner = GetActiveWindow();
                var result = MessageBox.Show(owner, _loc[options.message], _loc[options.title], MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                bool? boolResult;

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        boolResult = true; break;
                    case MessageBoxResult.No:
                        boolResult = false; break;
                    default:
                        boolResult = null; break;
                }

                return boolResult;
            });
        }

        public Task ShowErrorAsync(NotificationErrorOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var text = options.message;
                if (!string.IsNullOrEmpty(options.details))
                    text += Environment.NewLine + Environment.NewLine + _loc["Details_"] + options.details;

                var owner = GetActiveWindow();
                MessageBox.Show(owner, _loc[text], _loc[options.title], MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public Task ShowInfoAsync(NotificationInfoOptions options, CancellationToken ct = default)
        {
            return RunOnUIThreadAsync(() =>
            {
                var owner = GetActiveWindow();
                MessageBox.Show(owner, _loc[options.message], _loc[options.title], MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private Window? GetActiveWindow() => Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }
}
