namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPartHolder
    {
        PartTypeEnumVM? PartType { get; }
        IVMPart? Part { get; }
    }
}
