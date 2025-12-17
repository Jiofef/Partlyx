namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface ITypedVMPartHolder<TPart> : IVMPartHolder where TPart: IVMPart 
    {
        PartTypeEnumVM? IVMPartHolder.PartType { get => Part?.PartType; }
        IVMPart? IVMPartHolder.Part { get => Part; }
        new TPart? Part { get; }
    }
}
