using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels
{
    public interface IFocusable : IUidObjectViewModel
    {
        FocusableElementTypeEnum FocusableType { get; }
        FocusableItemUIState UiItem { get; }
    }
}
