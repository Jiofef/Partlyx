using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Events;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record RecipeComponentQuantityChangedEvent(
        Guid ComponentUid,
        Guid ResourceUid,
        RecipeComponentType ComponentType,
        double OldQuantity,
        double NewQuantity,
        object? ReceiverKey = null
    ) : IRoutedEvent;
}
