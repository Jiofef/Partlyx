using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface INotificationService
    {
        Task ShowErrorAsync(NotificationErrorOptions options, CancellationToken ct = default);
        Task ShowInfoAsync(NotificationInfoOptions options, CancellationToken ct = default);
        Task<bool> ShowYesNoConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default);
        /// <summary> Returns true on Yes, false on No and null on Cancel </summary>
        Task<bool?> ShowYesNoCancelConfirmAsync(NotificationConfirmOptions options, CancellationToken ct = default);
    }
}