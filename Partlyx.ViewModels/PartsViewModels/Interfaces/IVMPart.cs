using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPart : IDisposable, IIconHolderViewModel, IUidObjectViewModel, IFocusable
    {
        PartTypeEnumVM PartType { get; }
        PartsGlobalNavigations GlobalNavigations { get; }
        FocusableElementTypeEnum IFocusable.FocusableType { get => FocusableElementTypeEnum.RecipeHolder; }
    }
    public enum PartTypeEnumVM { Resource, Recipe, Component }
}
