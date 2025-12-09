using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPart : IDisposable, IIconHolderViewModel, IUidObjectViewModel
    {
        PartTypeEnumVM PartType { get; }
        PartItemUIState UiItem { get; }
        PartsGlobalNavigations GlobalNavigations { get; }
    }
    public enum PartTypeEnumVM { Resource, Recipe, Component }
}
