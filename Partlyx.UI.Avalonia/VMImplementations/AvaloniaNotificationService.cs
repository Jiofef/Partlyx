using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Partlyx.Core.Contracts;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class AvaloniaNotificationService : INotificationService
    {
        private readonly ILocalizationService _loc;
        public AvaloniaNotificationService(ILocalizationService loc) => _loc = loc;
        public async Task<bool> ShowYesNoConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default)
        {
            var owner = GetActiveWindow();
            var parameters = new MessageBoxStandardParams { ContentTitle = _loc[options.title], ContentMessage = _loc[options.message], ButtonDefinitions = ButtonEnum.YesNo, Icon = Icon.Question };
            var msgbox = MessageBoxManager.GetMessageBoxStandard(parameters);
            var result = await msgbox.ShowAsPopupAsync(owner);
            return result == ButtonResult.Yes;
        }
        public async Task<bool?> ShowYesNoCancelConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default)
        {
            var owner = GetActiveWindow();
            var parameters = new MessageBoxStandardParams { ContentTitle = _loc[options.title], ContentMessage = _loc[options.message], ButtonDefinitions = ButtonEnum.YesNoCancel, Icon = Icon.Question };
            var msgbox = MessageBoxManager.GetMessageBoxStandard(parameters);
            var result = await msgbox.ShowAsPopupAsync(owner);
            return result switch { ButtonResult.Yes => true, ButtonResult.No => false, _ => null };
        }
        public async Task ShowErrorAsync(NotificationErrorOptions options, CancellationToken ct = default)
        {
            var text = _loc[options.message];
            if (!string.IsNullOrEmpty(options.details)) text += Environment.NewLine + Environment.NewLine + _loc["Details"] + options.details;
            var owner = GetActiveWindow();
            var parameters = new MessageBoxStandardParams { ContentTitle = _loc[options.title], ContentMessage = text, ButtonDefinitions = ButtonEnum.Ok, Icon = Icon.Error };
            var msgbox = MessageBoxManager.GetMessageBoxStandard(parameters);
            await msgbox.ShowAsPopupAsync(owner);
        }
        public async Task ShowInfoAsync(NotificationInfoOptions options, CancellationToken ct = default)
        {
            var owner = GetActiveWindow();
            var parameters = new MessageBoxStandardParams { ContentTitle = _loc[options.title], ContentMessage = _loc[options.message], ButtonDefinitions = ButtonEnum.Ok, Icon = Icon.Info };
            var msgbox = MessageBoxManager.GetMessageBoxStandard(parameters);
            await msgbox.ShowAsPopupAsync(owner);
        }
        private Window? GetActiveWindow() => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault(w => w.IsActive);
    }
}