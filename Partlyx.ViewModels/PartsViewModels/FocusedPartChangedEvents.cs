using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record FocusedPartChangedEvent(PartTypeEnumVM? partType, Guid? selected);

    public record PartFocusedEvent(PartTypeEnumVM partType, Guid selected);

    public record PartUnfocusedEvent();
}
