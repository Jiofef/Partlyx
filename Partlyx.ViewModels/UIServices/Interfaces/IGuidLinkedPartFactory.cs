using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IGuidLinkedPartFactory
    {
        GuidLinkedPart<TPart> CreateLinkedPart<TPart>(Guid uid) where TPart : IVMPart;
    }
}