using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    /// <summary> A simplified version of SelectedParts for elements that need one single element of one single type </summary>
    public interface IFocusedPart
    {
        IVMPart? FocusedPart { get; }
        PartTypeEnumVM? SelectedPartType { get; }

        bool HasFocusedPart { get; }
        void FocusPart(IVMPart? part);
    }

    public interface IIsolatedFocusedPart : IFocusedPart
    {

    }
    public interface IGlobalFocusedPart : IFocusedPart
    {

    }
}