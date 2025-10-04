using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels
{
    public record PartsTargetInteractionInfo<TPart, TTargetPart>(List<TPart> Parts, TTargetPart Target) 
        where TPart : IVMPart where TTargetPart : IVMPart;

    public record PartsMultipleTargetsInteractionInfo<TPart, TTargetPart>(List<TPart> Parts, List<TTargetPart> Targets)
        where TPart : IVMPart where TTargetPart : IVMPart;
}