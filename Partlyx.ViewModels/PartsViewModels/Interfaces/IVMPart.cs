using Partlyx.ViewModels.GlobalNavigations;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.PartsViewModels.Interfaces
{
    public interface IVMPart : IDisposable
    {
        PartTypeEnumVM PartType { get; }
        IconViewModel Icon { get; }
        Guid Uid { get; }
        PartItemUIState UiItem { get; }
        PartsGlobalNavigations GlobalNavigations { get; }
    }

    public enum PartTypeEnumVM { Resource, Recipe, Component }
}
