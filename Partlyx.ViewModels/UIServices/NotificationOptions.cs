using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices
{
    public record NotificationErrorOptions(string title, string message, string? details = null);
    public record NotificationInfoOptions(string title, string message);
    public record NotificationConfirmOptions(string title, string message);
}
