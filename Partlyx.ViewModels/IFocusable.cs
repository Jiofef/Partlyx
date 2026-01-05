using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels
{
    public interface IFocusable
    {
        FocusableElementTypeEnum FocusableType { get; }
        FocusableItemUIState UiItem { get; }

        Guid Uid { get; }
    }
}
