using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record GlobalFocusedPartChangedEvent(PartTypeEnumVM? FocusedPartType, Guid? FocusedPartUid, PartTypeEnumVM? PreviousSelectedPartType, Guid? PreviousSelectedPartUid);

    public record GlobalPartFocusedEvent(PartTypeEnumVM FocusedPartType, Guid FocusedPartUid, PartTypeEnumVM? PreviousSelectedPartType, Guid? PreviousSelectedPartUid);

    public record GlobalPartUnfocusedEvent();
}
