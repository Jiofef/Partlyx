using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record GlobalFocusedElementChangedEvent(IFocusable? NewFocused, IFocusable? PreviousFocused);

    public record GlobalElementFocusedEvent(IFocusable NewFocused, IFocusable? PreviousFocused);

    public record GlobalElementUnfocusedEvent();
}
