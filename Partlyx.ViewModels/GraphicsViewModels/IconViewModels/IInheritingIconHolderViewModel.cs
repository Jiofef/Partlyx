using Partlyx.Core.VisualsInfo;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public interface IInheritingIconHolderViewModel : IIconHolderViewModel
    {
        public InheritedIcon.InheritedIconParentTypeEnum InheritedIconParentDefaultType { get; }
        public Guid? InheritedIconDefaultParentUid { get; }
    }
}
