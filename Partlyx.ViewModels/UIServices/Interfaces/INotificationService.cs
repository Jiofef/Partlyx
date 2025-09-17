using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface INotificationService
    {
        Task ShowErrorAsync(string title, string message, string? details = null, CancellationToken ct = default);
        Task ShowInfoAsync(string title, string message, CancellationToken ct = default);
        Task<bool> ShowConfirmAsync(string title, string message, CancellationToken ct = default);
    }
}
